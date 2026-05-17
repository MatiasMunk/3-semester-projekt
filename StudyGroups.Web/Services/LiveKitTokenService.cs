using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace StudyGroups.Web.Services;

public class LiveKitTokenService
{
    private readonly LiveKitOptions _options;

    public LiveKitTokenService(IOptions<LiveKitOptions> options)
    {
        _options = options.Value;
    }

    public string ServerUrl => _options.Url;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.Url) &&
        !string.IsNullOrWhiteSpace(_options.ApiKey) &&
        !string.IsNullOrWhiteSpace(_options.ApiSecret) &&
        !_options.ApiKey.Contains("replace", StringComparison.OrdinalIgnoreCase) &&
        !_options.ApiSecret.Contains("replace", StringComparison.OrdinalIgnoreCase);

    public string CreateRoomToken(string roomName, string identity, string displayName)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("LiveKit is not configured. Set LiveKit:Url, LiveKit:ApiKey and LiveKit:ApiSecret in appsettings or user secrets.");

        var now = DateTimeOffset.UtcNow;
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object>
        {
            ["iss"] = _options.ApiKey,
            ["sub"] = identity,
            ["name"] = displayName,
            ["nbf"] = now.AddSeconds(-10).ToUnixTimeSeconds(),
            ["exp"] = now.AddHours(2).ToUnixTimeSeconds(),
            ["video"] = new Dictionary<string, object>
            {
                ["room"] = roomName,
                ["roomJoin"] = true,
                ["canPublish"] = true,
                ["canSubscribe"] = true,
                ["canPublishData"] = true
            }
        };

        var encodedHeader = Base64Url(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signingInput = $"{encodedHeader}.{encodedPayload}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ApiSecret));
        var signature = Base64Url(hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput)));

        return $"{signingInput}.{signature}";
    }

    private static string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
