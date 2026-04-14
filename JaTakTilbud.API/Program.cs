using JaTakTilbud.API.Auth;
using JaTakTilbud.Core.Interfaces;
using JaTakTilbud.Infrastructure.Data;
using JaTakTilbud.Infrastructure.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Services
// -----------------------------

// Controllers
builder.Services.AddControllers();

// Swagger (with JWT support)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JaTakTilbud API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });
});

// -----------------------------
// Database (Dapper)
// -----------------------------

// Configure Database connection factory using the connection string from configuration in appsettings.json or other environment variables
builder.Services.AddSingleton(new DbConnectionFactory(
    builder.Configuration.GetConnectionString("DefaultConnection")!
));

// scoped services
builder.Services.AddScoped<ICampaignService, CampaignService>();

// -----------------------------
// OpenID Connect
// -----------------------------

OpenIdService oidcConfig = new();
oidcConfig.ConfigureBuilder(builder);

// -----------------------------
// Forwarded headers (Docker / proxy safe)
// -----------------------------

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// -----------------------------
// Build app
// -----------------------------

var app = builder.Build();

// -----------------------------
// Middleware
// -----------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

// IMPORTANT ORDER
app.UseAuthentication();
app.UseAuthorization();

// OpenID endpoints (login/logout etc.)
oidcConfig.ConfigureApp(app);

app.MapControllers();

// -----------------------------
// Run
// -----------------------------

app.Run();