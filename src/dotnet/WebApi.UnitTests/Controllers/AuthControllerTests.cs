using JfYu.WeChat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;
using WebApi.Constants;
using WebApi.Controllers;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Request;
using WebApi.Model.Response;
using WebApi.Options;
using WebApi.Services.Interfaces;

namespace WebApi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMiniProgram> _mockMiniProgram;
        private readonly IOptions<JwtSettings> _mockJwtSettings;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockJwtService = new Mock<IJwtService>();
            _mockUserService = new Mock<IUserService>();
            _mockMiniProgram = new Mock<IMiniProgram>();
            var jwtSettings = new JwtSettings { Audience = "test", Issuer = "test", SecretKey = "test", Expires = 3600 };

            _mockJwtSettings = Microsoft.Extensions.Options.Options.Create(jwtSettings);
            _controller = new AuthController(_mockJwtService.Object, _mockUserService.Object, _mockJwtSettings);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                UserName = "admin",
                Password = "admin!@#p",
                Platform = PlatformEnum.Web
            };
            var expectedToken = "generated.jwt.token";
            var user = new User { Id = 1, UserName = "admin", Role = UserRoleEnum.Admin };
            _mockUserService
                .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(user);
            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()))
                .Returns(expectedToken);

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var value = okResult!.Value;
            value.Should().NotBeNull();

            var json = JsonSerializer.Serialize(value);
            var jsonElement = JsonSerializer.Deserialize<BaseResponse<LoginResponse>>(json);

            jsonElement?.Data?.AccessToken.Should().Be(expectedToken);
            jsonElement?.Data?.Username.Should().Be("admin");
            jsonElement?.Data?.ExpiresIn.Should().Be(3600);

            _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidUsername_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                UserName = "wronguser",
                Password = "admin!@#p",
                Platform = PlatformEnum.Web
            };
            _mockUserService
                .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                UserName = "admin",
                Password = "wrongpassword",
                Platform = PlatformEnum.Web
            };
            _mockUserService
                .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnInvalidCredentialsError()
        {
            // Arrange
            var request = new LoginRequest
            {
                UserName = "wronguser",
                Password = "wrongpassword",
                Platform = PlatformEnum.Web
            };
            _mockUserService
                .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult!.Value;
            errorResponse.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_ShouldGenerateTokenWithCorrectClaims()
        {
            // Arrange
            var request = new LoginRequest
            {
                UserName = "admin",
                Password = "admin!@#p",
                Platform = PlatformEnum.Web
            };
            var user = new User { Id = 1, UserName = "admin", Role = UserRoleEnum.Admin };
            _mockUserService
                .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(user);
            IEnumerable<Claim>? capturedClaims = null;
            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()))
                .Callback<IEnumerable<Claim>>(claims => capturedClaims = claims)
                .Returns("token");

            // Act
            await _controller.LoginAsync(request);

            // Assert
            capturedClaims.Should().NotBeNull();
            capturedClaims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "admin");
            capturedClaims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        [Fact]
        public void Validate_WithAuthenticatedUser_ShouldReturnUserInfo()
        {
            // Arrange
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = _controller.Validate();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var value = okResult!.Value;

            var json = JsonSerializer.Serialize(value);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            var data = jsonElement.GetProperty("Data");
            data.GetProperty("Username").GetString().Should().Be("testuser");
            data.GetProperty("Role").GetString().Should().Be("Admin");
            data.GetProperty("IsAuthenticated").GetBoolean().Should().BeTrue();
        }

        [Fact]
        public void Validate_WithUnauthenticatedUser_ShouldReturnNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.Validate();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var value = okResult!.Value;

            var json = JsonSerializer.Serialize(value);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            var data = jsonElement.GetProperty("Data");
            data.GetProperty("IsAuthenticated").GetBoolean().Should().BeFalse();
        }

    }
}
