using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using HireConnect.Job.Data;
using HireConnect.Job.Elasticsearch;
using HireConnect.Job.Middleware;
using HireConnect.Job.Repositories;
using HireConnect.Job.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ── 1. Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── 2. PostgreSQL via EF Core (Neon) ─────────────────────────────────────────
builder.Services.AddDbContext<JobDbContext>(options =>
    options.UseNpgsql(
        config.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

// ── 3. Repository & Service DI ────────────────────────────────────────────────
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobServiceImpl>();

// ── 4. Elasticsearch ──────────────────────────────────────────────────────────
var esSection = config.GetSection("Elasticsearch");
var esUri = esSection["Uri"] ?? "http://localhost:9200";
var esUsername = esSection["Username"];
var esPassword = esSection["Password"];
var esCloudId = esSection["CloudId"];
var esApiKey = esSection["ApiKey"];

ElasticsearchClient elasticClient;

if (!string.IsNullOrWhiteSpace(esCloudId) && !string.IsNullOrWhiteSpace(esApiKey))
{
    Console.WriteLine($"[Elasticsearch] Connecting to Elastic Cloud (CloudId: {esCloudId.Substring(0, 10)}...)");
    elasticClient = new ElasticsearchClient(new ElasticsearchClientSettings(esCloudId, new ApiKey(esApiKey)));
}
else if (!string.IsNullOrWhiteSpace(esUsername) && !string.IsNullOrWhiteSpace(esPassword))
{
    Console.WriteLine("[Elasticsearch] Connecting to Self-hosted with Basic Auth");
    var settings = new ElasticsearchClientSettings(new Uri(esUri))
        .Authentication(new BasicAuthentication(esUsername, esPassword))
        .RequestTimeout(TimeSpan.FromSeconds(10));
    elasticClient = new ElasticsearchClient(settings);
}
else
{
    Console.WriteLine($"[Elasticsearch] Falling back to Local Dev (Uri: {esUri})");
    var settings = new ElasticsearchClientSettings(new Uri(esUri))
        .RequestTimeout(TimeSpan.FromSeconds(10));
    elasticClient = new ElasticsearchClient(settings);
}

builder.Services.AddSingleton(elasticClient);
builder.Services.AddSingleton<IJobElasticsearchService, JobElasticsearchService>();

// ── 5. MassTransit with RabbitMQ ──────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitUri = config["RabbitMQ:Uri"];
        if (!string.IsNullOrEmpty(rabbitUri))
        {
            cfg.Host(new Uri(rabbitUri));
        }
        else
        {
            var rabbitConfig = config.GetSection("RabbitMQ");
            cfg.Host(rabbitConfig["Host"] ?? "localhost", h =>
            {
                h.Username(rabbitConfig["Username"] ?? "guest");
                h.Password(rabbitConfig["Password"] ?? "guest");
            });
        }
    });
});

// ── 5. JWT Authentication ─────────────────────────────────────────────────────
var jwtSection = config.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // No leeway — tokens expire exactly on time
    };
});

builder.Services.AddAuthorization();

// ── 6. CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ── 7. Swagger / OpenAPI ──────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HireConnect — Job Service API",
        Version = "v1",
        Description = "Manages the full lifecycle of job postings for the HireConnect platform. " +
                      "Supports creation, retrieval, Elasticsearch-powered full-text search, filtering, " +
                      "update, and deletion of job listings.",
        Contact = new OpenApiContact { Name = "HireConnect Team" }
    });

    // JWT support in Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {your-jwt-token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// ── 8. Health Checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(config.GetConnectionString("DefaultConnection")!);

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── 9. Database Migration on startup ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JobDbContext>();
    await db.Database.MigrateAsync();

    // Ensure Elasticsearch index exists
    var esService = scope.ServiceProvider.GetRequiredService<IJobElasticsearchService>();
    try
    {
        await esService.EnsureIndexAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Elasticsearch index initialization failed. Service will continue.");
    }
}

// ── 10. Middleware Pipeline ───────────────────────────────────────────────────
app.UseGlobalExceptionHandler(); // must be first

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HireConnect Job Service v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
