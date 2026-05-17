namespace StudyGroups.API.Middleware;

public class AdminOnlyMiddleware
{
    private const string AdminApiKeyHeader = "X-Admin-Key";
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public AdminOnlyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresAdmin(context.Request))
        {
            await _next(context);
            return;
        }

        if (IsAdminByClaims(context) || IsAdminByApiKey(context))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Admin access required.");
    }

    private static bool RequiresAdmin(HttpRequest request)
    {
        var path = request.Path.Value ?? string.Empty;

        if (path.StartsWith("/api/users", StringComparison.OrdinalIgnoreCase))
            return true;

        if (path.StartsWith("/api/categories", StringComparison.OrdinalIgnoreCase))
            return !HttpMethods.IsGet(request.Method);

        return false;
    }

    private static bool IsAdminByClaims(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true)
            return false;

        return user.Claims.Any(claim =>
            claim.Type.Equals("roles", StringComparison.OrdinalIgnoreCase) &&
            (claim.Value.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
             claim.Value.Equals("warlock", StringComparison.OrdinalIgnoreCase)));
    }

    private bool IsAdminByApiKey(HttpContext context)
    {
        var configuredKey = _configuration["AdminApiKey"];

        if (string.IsNullOrWhiteSpace(configuredKey))
            return false;

        if (!context.Request.Headers.TryGetValue(AdminApiKeyHeader, out var providedKey))
            return false;

        return string.Equals(providedKey.ToString(), configuredKey, StringComparison.Ordinal);
    }
}
