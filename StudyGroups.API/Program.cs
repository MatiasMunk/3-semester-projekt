using StudyGroups.Core.Interfaces;
using StudyGroups.Infrastructure.Data;
using StudyGroups.Infrastructure.Services;
using Microsoft.AspNetCore.HttpOverrides;
using StudyGroups.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Services
// -----------------------------

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------------
// Database (Dapper)
// -----------------------------

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Configure Database connection factory using the connection string from configuration in appsettings.json or other environment variables
builder.Services.AddSingleton<IDbConnectionFactory>(
    new DbConnectionFactory(connectionString)
);

// scoped services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStudySessionService, StudySessionService>();
builder.Services.AddScoped<ICategoriesService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPrivateMessageService, PrivateMessageService>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();

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

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyGroups API v1");
});

// Custom auth is currently session/API-key based, not ASP.NET auth schemes.
// Do not call UseAuthentication/UseAuthorization unless AddAuthentication is configured.

// Admin protection for category mutations and user management.
// Allows the configured X-Admin-Key used by the local WinForms admin client.
app.UseMiddleware<AdminOnlyMiddleware>();

app.MapControllers();

// -----------------------------
// Run
// -----------------------------

app.Run();