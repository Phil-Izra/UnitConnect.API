using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UnitConnect.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetResidentId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    public static Guid GetComplexId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("complex_id")!);
}
