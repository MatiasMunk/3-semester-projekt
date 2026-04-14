using JaTakTilbud.Web.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure for OpenID
OpenIdService oidcConfig = new();
oidcConfig.ConfigureBuild(builder);

var app = builder.Build();

// Configure for OpenID
oidcConfig.ConfigureApp(app);

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseForwardedHeaders();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
