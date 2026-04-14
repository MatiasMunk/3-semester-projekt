using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace JaTakTilbud.Web.Auth
{
    public class OpenIdService
    {
        // Configuration priority from highest to lowest
        // Highest: Command line arguments
        //        : Non-prefixed environment variables
        //        : User secrets from the .NET User Secrets Manager
        //        : Any appsettings.{ ENVIRONMENT_NAME }.json files
        //        : The appsettings.json file
        // Lowest : Fallback to the host configuration

        /// <summary>
        /// Setting up OpenIDConnect authentication (Program.cs)
        /// </summary>
        /// <param name="builder">WebApplicationBuilder</param>

        public void ConfigureBuild(WebApplicationBuilder builder)
        {
            // Add services to the container.
            MyConfiguration.Set(builder.Configuration);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = builder.Configuration["OpenIDRealmURI"];
                options.ClientId = builder.Configuration["OpenIDClient"];
                options.ClientSecret = builder.Configuration["OpenIDSecret"];
                options.CallbackPath = "/signin-oidc";
                //options.CallbackPath = "/signin-external";

                // Authorization Code flow (recommended)
                options.ResponseType = "code";
                options.SaveTokens = true;                // store tokens in auth properties
                options.GetClaimsFromUserInfoEndpoint = true;
                options.MapInboundClaims = false;

                // Scopes - clear then add what's needed
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("ucn");
                options.Scope.Add("email");
                options.Scope.Add("offline_access"); // to get refresh tokens

                // Map claim types if needed
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "roles"
                };

                // Optional: events for logging / error handling
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = context =>
                    {
                        // e.g. add custom claims or logging
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Error?message=" + Uri.EscapeDataString(context.Exception.Message));
                        return Task.CompletedTask;
                    },
                    // added29.10.2025
                    OnSignedOutCallbackRedirect = context =>
                    {
                        context.Response.Redirect(context.Options.SignedOutRedirectUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    }

                };
            });
            // Change here for authorization policies
            builder.Services
                   .AddAuthorizationBuilder()
                   .AddPolicy("read_access", builder =>
                   {
                       // claim, list of acceptable values
                       builder.RequireClaim("myClaim", "MyClaimValueRO1", "MyClaimValueRO2");
                   })
                   .AddPolicy("write_access", builder =>
                   {
                       builder.RequireClaim("myClaim", "MyClaimValueRW1", "MyClaimValueRW2");
                   });
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

        }

        /// <summary>
        /// Map synthetic endpoints for authentication
        /// </summary>
        /// <param name="app">WebApplication</param>
        public void ConfigureApp(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Synthetic endpoint for authentication
            // Call this endpoint to start login sequence.
            // Something like this will do: <a href="authentication/login" class="btn btn-warning">Login</a>
            app.MapGet("/authentication/login", ()
                => TypedResults.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" }))
                .AllowAnonymous();
            app.MapGet("/authentication/logout", ()
                => TypedResults.SignOut(
                    new AuthenticationProperties { RedirectUri = "/" }))
                .AllowAnonymous();

        }

        public async Task<JwtSecurityToken?> GetJwtPayloadAsync(HttpContext myContext)
        {
            var token = await myContext.GetTokenAsync("access_token");

            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }

        public async Task<string?> GetJwtClaimAsync(HttpContext context, string claimType)
        {
            var jwt = await GetJwtPayloadAsync(context);

            return jwt?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

    }

    static public class MyConfiguration
    {
        static ConfigurationManager? _config;
        static public void Set(ConfigurationManager config)
        {
            _config = config;
        }

        static public ConfigurationManager? Get()
        {
            return _config;
        }

    }
}
