using Microsoft.AspNetCore.Http;

namespace StudyGroups.Web.Extensions;

public static class SessionExtensions
{
    public static int? GetUserId(this HttpContext context)
    {
        return context.Session.GetInt32("UserId");
    }

    public static string GetDisplayName(this HttpContext context)
    {
        return context.Session.GetString("Username")
               ?? $"User {context.GetUserId() ?? 0}";
    }
}