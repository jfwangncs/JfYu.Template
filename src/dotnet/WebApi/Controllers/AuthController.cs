
using JfYu.WeChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApi.Constants;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Request;
using WebApi.Model.Response;
using WebApi.Options;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IJwtService jwtService, IUserService userService, IOptions<JwtSettings> jwtSettings) : CustomController
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly IUserService _userService = userService;
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResponse<LoginResponse>), 200)]
        public async Task<IActionResult> LoginAsync([FromBody][Required] LoginRequest request)
        {
            User? user = await _userService.LoginAsync(request);
            if (user == null)
                return BadRequest(ErrorCode.InvalidCredentials);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach (var item in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, item.Name));

            var token = _jwtService.GenerateToken(claims);

            return Ok(new LoginResponse
            {
                Id = user.Id,
                AccessToken = token,
                RealName = user.RealName ?? user.UserName,
                ExpiresIn = _jwtSettings.Expires,
                Roles = user.Roles.Select(r => r.Name).ToList()
            });
        }
        [HttpGet("validate")]
        [Authorize]
        public IActionResult Validate()
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                Username = username,
                Role = role,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }
    }
}
