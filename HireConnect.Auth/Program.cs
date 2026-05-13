using System.Text;
using HireConnect.Auth.Data;
using HireConnect.Auth.Repositories;
using HireConnect.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// ── Database: Neon PostgreSQL ─────────────────────────────────────
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Dependency Injection ──────────────────────────────────────────
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthServiceImpl>();

// ── MassTransit with RabbitMQ ──────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitUri = builder.Configuration["RabbitMQ:Uri"];
        if (!string.IsNullOrEmpty(rabbitUri))
        {
            cfg.Host(new Uri(rabbitUri));
        }
        else
        {
            var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
            cfg.Host(rabbitConfig["Host"] ?? "localhost", h =>
            {
                h.Username(rabbitConfig["Username"] ?? "guest");
                h.Password(rabbitConfig["Password"] ?? "guest");
            });
        }
    });
});

// ── Authentication ────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme       = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer           = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidateAudience         = true,
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            ClockSkew                = TimeSpan.Zero
        };
    })
    .AddGitHub(options =>
    {
        options.ClientId     = builder.Configuration["OAuth:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["OAuth:GitHub:ClientSecret"]!;
        options.Scope.Add("user:email");
        options.CallbackPath = "/api/auth/callback/github";
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            var uriBuilder = new UriBuilder(context.RedirectUri);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["redirect_uri"] = "https://hireconnect-gateway.onrender.com/api/auth/callback/github";
            uriBuilder.Query = query.ToString();
            context.Response.Redirect(uriBuilder.ToString());
            return Task.CompletedTask;
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId     = builder.Configuration["OAuth:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"]!;
        options.CallbackPath = "/api/auth/callback/google";
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            var uriBuilder = new UriBuilder(context.RedirectUri);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["redirect_uri"] = "https://hireconnect-gateway.onrender.com/api/auth/callback/google";
            uriBuilder.Query = query.ToString();
            context.Response.Redirect(uriBuilder.ToString());
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger with JWT support ──────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste your JWT here"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));


// ── Health check ──────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// ── Auto migrate on startup ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.Run();