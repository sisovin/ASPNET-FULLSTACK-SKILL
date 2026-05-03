using System.Security.Claims;
using System.Text.Json;

namespace PhoneShop.Client.Auth;

public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts   = jwt.Split('.');
        if (parts.Length != 3)
            return Enumerable.Empty<Claim>();

        var payload  = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);

        var kvp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)
                  ?? new Dictionary<string, JsonElement>();

        var claims = new List<Claim>();
        foreach (var (key, value) in kvp)
        {
            // Expand array claims (e.g. roles)
            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                    claims.Add(new Claim(key, item.ToString()));
            }
            else
            {
                claims.Add(new Claim(key, value.ToString()));
            }
        }
        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "=";  break;
        }
        return Convert.FromBase64String(base64);
    }
}
