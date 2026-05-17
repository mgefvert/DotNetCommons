using System.Security.Claims;

namespace DotNetCommons.Security;

public static class ClaimsPrincipalExtensions
{
    public static string? GetClaim(this ClaimsPrincipal? principal, string claimType)
    {
        return principal?.FindFirst(claimType)?.Value;
    }

    public static int? GetClaimInt32(this ClaimsPrincipal? principal, string claimType)
    {
        var value = principal.GetClaim(claimType);
        return !string.IsNullOrEmpty(value) && int.TryParse(value, out var result) ? result : null;
    }

    public static uint? GetClaimUInt32(this ClaimsPrincipal? principal, string claimType)
    {
        var value = principal.GetClaim(claimType);
        return !string.IsNullOrEmpty(value) && uint.TryParse(value, out var result) ? result : null;
    }

    public static long? GetClaimInt64(this ClaimsPrincipal? principal, string claimType)
    {
        var value = principal.GetClaim(claimType);
        return !string.IsNullOrEmpty(value) && long.TryParse(value, out var result) ? result : null;
    }

    public static ulong? GetClaimUInt64(this ClaimsPrincipal? principal, string claimType)
    {
        var value = principal.GetClaim(claimType);
        return !string.IsNullOrEmpty(value) && ulong.TryParse(value, out var result) ? result : null;
    }
}