using Microsoft.AspNetCore.Localization;
using StudyGroups.Http;
using StudyGroups.Http.Interfaces;
using StudyGroups.Http.Services;
using StudyGroups.Web.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ── HTTP client layer ────────────────────────────────────────────
// Read the API base URL from configuration so it can be changed
// per environment without recompiling.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException(
        "ApiBaseUrl is not configured. Add it to appsettings.json.");

// In development the API uses a self-signed localhost cert.
// ApiTrustDevCert=true bypasses cert validation so the HttpClient
// can reach https://localhost without a full trust-store setup.
// Keep this false (or omit it) in staging/production.
var trustDevCert = builder.Configuration.GetValue<bool>("ApiTrustDevCert");

builder.Services.AddSingleton(new ApiClient(apiBaseUrl, trustDevCert));

builder.Services.AddScoped<IStudySessionApi, StudySessionApi>();
builder.Services.AddScoped<ICategoryApi, CategoryApi>();
builder.Services.AddScoped<IAuthApi, AuthApi>();
builder.Services.AddScoped<IPrivateMessageApi, PrivateMessageApi>();
builder.Services.AddScoped<IFriendRequestApi, FriendRequestApi>();

builder.Services.Configure<LiveKitOptions>(builder.Configuration.GetSection("LiveKit"));
builder.Services.AddSingleton<LiveKitTokenService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("da")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");

    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

var locOptions = app.Services.GetRequiredService<
    Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();

app.UseRequestLocalization(locOptions.Value);

app.UseHttpsRedirection();

app.UseSession();

app.UseStaticFiles();

app.UseRouting();

app.UseForwardedHeaders();
// Custom auth uses MVC session state for now. No ASP.NET authentication scheme is configured.
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
