using System.Security.Claims;

namespace JfYu.WebApi.Template.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(IEnumerable<Claim> claims);
        ClaimsPrincipal ValidateToken(string token);
    }
}