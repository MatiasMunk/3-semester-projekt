using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;


// Required NuGet Packages
// Microsoft.AspNet.WebApi.Core
// Microsoft.AspNetCore.Authentication.JwtBearer
// Microsoft.AspNetCore.Authentication.OpenIdConnect
// Microsoft.IdentityModel.Protocols.OpenIdConnect


namespace JaTakTilbud.API.Auth
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

        public void ConfigureBuilder(WebApplicationBuilder builder)
        {
            MyConfiguration.Set(builder.Configuration);

            // >>> This adds the authentication service
            builder.Services
                   .AddAuthentication()
                   .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                   {
                       // OpenIDRealmURI coud be "https://auth.a.ucnit.eu/realms/xOIDCx"
                       options.Authority = builder.Configuration["OpenIDRealmURI"];
                       options.Audience = "account";
                       options.MapInboundClaims = false;
                       options.Events = new JwtBearerEvents
                       {
                           OnMessageReceived = context =>
                           {
                               var token = context.Request.Headers.Authorization.ToString();
                               var path = context.Request.Path.ToString();

                               if (!string.IsNullOrEmpty(token))
                               {
                                   Console.WriteLine("Access token");
                                   Console.WriteLine($"URL: {path}");
                                   Console.WriteLine($"Token present: {!string.IsNullOrEmpty(token)}");
                               }
                               else
                               {
                                   Console.WriteLine("Access token");
                                   Console.WriteLine($"URL: {path}");
                                   Console.WriteLine("Token: No access token provided\n");
                               }

                               return Task.CompletedTask;
                           },

                           OnTokenValidated = context =>
                           {
                               Console.WriteLine("\nClaims from the access token");

                               if (context.Principal != null)
                               {
                                   foreach (var claim in context.Principal.Claims)
                                   {
                                       Console.WriteLine($"{claim.Type} - {claim.Value}");
                                   }
                               }

                               Console.WriteLine();
                               return Task.CompletedTask;
                           }
                       };

                   });
            // <<<

            // >>> This adds the authorization service
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireClaim("roles", "admin", "warlock"));
            });

            ConfigureBuilderOpenAPI(builder);

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

        }

        private void ConfigureBuilderOpenAPI(WebApplicationBuilder builder)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            // Add Swagger with Authorization option
            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/HEAD/docs/configure-and-customize-swaggergen.md#add-security-definitions-and-requirements
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1"
                });
                options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });
            });
            // Add Swagger with Authorization option
        }

        private void ConfigureAppOpenAPI(WebApplication app)
        {
            app.MapSwagger();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("v1/swagger.json", "Your Name Here");
            });
        }



        /// <summary>
        /// </summary>
        /// <param name="app">WebApplication</param>
        public void ConfigureApp(WebApplication app)
        {
            if (app.Environment.IsDevelopment()) ConfigureAppOpenAPI(app);
        }

        public async Task<JwtSecurityToken?> GetJwtPayloadAsync(HttpContext context)
        {
            var token = await context.GetTokenAsync("access_token");

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
