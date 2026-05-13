var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS to allow frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");

// Map YARP routes
app.MapReverseProxy();

app.Run();
