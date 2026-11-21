using System.Security.Claims;

namespace PlacementApi.Auth;

public static class UserClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
        => int.Parse(user.FindFirst("sub")!.Value);

    public static int? GetStudentId(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirst("studentId")?.Value, out var id) ? id : null;
}