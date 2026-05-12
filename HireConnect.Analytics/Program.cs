using HireConnect.Analytics.Consumers;
using HireConnect.Analytics.Data;
using HireConnect.Analytics.Middleware;
using HireConnect.Analytics.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ── 1. Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── 2. PostgreSQL via EF Core ─────────────────────────────────────────────────
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(
        config.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

// ── 3. Service DI ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAnalyticsService, AnalyticsServiceImpl>();

// ── 4. MassTransit with RabbitMQ ──────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AnalyticsEventConsumer>();

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

        cfg.ReceiveEndpoint("analytics-service-queue", e =>
        {
            e.ConfigureConsumer<AnalyticsEventConsumer>(context);
        });
    });
});

// ── 3. JWT Authentication ─────────────────────────────────────────────────────
var jwtSection = config.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSection["Issuer"],
        ValidAudience            = jwtSection["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew                = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

// ── 4. CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── 5. Swagger / OpenAPI ──────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "HireConnect — Analytics Service API",
        Version     = "v1",
        Description = "Provides recruiter-level and platform-level hiring metrics, pipeline stats, and report aggregations."
    });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name          = "Authorization",
        Description   = "Enter: Bearer {your-jwt-token}",
        In            = ParameterLocation.Header,
        Type          = SecuritySchemeType.ApiKey,
        Scheme        = "Bearer",
        BearerFormat  = "JWT",
        Reference     = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
});

// ── 7. Health Checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(config.GetConnectionString("DefaultConnection")!);

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── 8. Auto-Migrate ───────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    await db.Database.MigrateAsync();
}

// ── 7. Middleware Pipeline ────────────────────────────────────────────────────
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HireConnect Analytics Service v1");
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
