using JaTakTilbud.API.Auth;
using JaTakTilbud.Core.Interfaces;
using JaTakTilbud.Infrastructure.Data;
using JaTakTilbud.Infrastructure.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Services
// -----------------------------

// Controllers
builder.Services.AddControllers();

// -----------------------------
// Database (Dapper)
// -----------------------------

// Configure Database connection factory using the connection string from configuration in appsettings.json or other environment variables
builder.Services.AddSingleton(new DbConnectionFactory(
    builder.Configuration.GetConnectionString("DefaultConnection")!
));

// scoped services
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IProductService, ProductService>();

// -----------------------------
// OpenID Connect (Swagger + Auth (Centralized))
// -----------------------------
var oidcConfig = new OpenIdService();
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