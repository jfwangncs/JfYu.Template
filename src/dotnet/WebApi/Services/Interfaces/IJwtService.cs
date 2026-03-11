using System.Security.Claims;

namespace WebApi.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(IEnumerable<Claim> claims);
        ClaimsPrincipal ValidateToken(string token);
    }
}