using HireConnect.Notification.Consumers;
using HireConnect.Notification.Data;
using HireConnect.Notification.Middleware;
using HireConnect.Notification.Repositories;
using HireConnect.Notification.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ── 1. Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── 2. PostgreSQL via EF Core ─────────────────────────────────────────────────
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(
        config.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

// ── 3. Repository & Service DI ────────────────────────────────────────────────
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();
builder.Services.AddScoped<IEmailService, EmailServiceImpl>();

// ── 4. MassTransit with RabbitMQ ──────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<JobPostedConsumer>();
    x.AddConsumer<ApplicationSubmittedConsumer>();
    x.AddConsumer<ApplicationStatusChangedConsumer>();
    x.AddConsumer<InterviewScheduledConsumer>();
    x.AddConsumer<UserRegisteredConsumer>();

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

        cfg.ReceiveEndpoint("notification-service-queue", e =>
        {
            e.ConfigureConsumer<JobPostedConsumer>(context);
            e.ConfigureConsumer<ApplicationSubmittedConsumer>(context);
            e.ConfigureConsumer<ApplicationStatusChangedConsumer>(context);
            e.ConfigureConsumer<InterviewScheduledConsumer>(context);
            e.ConfigureConsumer<UserRegisteredConsumer>(context);
        });
    });
});

// ── 4. JWT Authentication ─────────────────────────────────────────────────────
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

// ── 5. CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── 6. Swagger / OpenAPI ──────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "HireConnect — Notification Service API",
        Version     = "v1",
        Description = "Manages in-app notifications and email alerts for HireConnect users."
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
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();
}

// ── 9. Middleware Pipeline ────────────────────────────────────────────────────
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HireConnect Notification Service v1");
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
