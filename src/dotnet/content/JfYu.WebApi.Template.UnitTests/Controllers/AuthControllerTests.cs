using JfYu.Data.Model;
using JfYu.WebApi.Template.Constants;
using JfYu.WebApi.Template.Controllers;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Exceptions;
using JfYu.WebApi.Template.Model;
using JfYu.WebApi.Template.Model.User;
using JfYu.WebApi.Template.Options;
using JfYu.WebApi.Template.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace JfYu.WebApi.Template.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private static (AuthController ctrl, Mock<IJwtService> jwtMock, Mock<IUserService> userMock,
            Mock<ICurrentUser> currentUserMock, Mock<IPermissionService> permMock, Mock<ILoginLogService> logMock)
            CreateController()
        {
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

            var jwtMock = new Mock<IJwtService>();
            var userMock = new Mock<IUserService>();
            var permMock = new Mock<IPermissionService>();
            var currentUserMock = new Mock<ICurrentUser>();
            var logMock = new Mock<ILoginLogService>();

            var settings = new JwtSettings { SecretKey = "key", Issuer = "i", Audience = "a", Expires = 60 };

            var ctrl = new AuthController(
                jwtMock.Object, userMock.Object, Microsoft.Extensions.Options.Options.Create(settings),
                currentUserMock.Object, logMock.Object);
            ctrl.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            return (ctrl, jwtMock, userMock, currentUserMock, permMock, logMock);
        }

        private static BaseResponse<object>? GetOkValue(IActionResult result)
            => result.Should().BeOfType<OkObjectResult>().Subject.Value as BaseResponse<object>;

        private static BaseResponse<object>? GetBadValue(IActionResult result)
            => result.Should().BeOfType<BadRequestObjectResult>().Subject.Value as BaseResponse<object>;

        // ─── LoginAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsOkWithToken()
        {
            var (ctrl, jwtMock, userMock, _, _, _) = CreateController();
            var user = new User { Id = 1, UserName = "admin" };
            userMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(user);
            jwtMock.Setup(s => s.GenerateToken(It.IsAny<IEnumerable<Claim>>())).Returns("test.token.here");

            var result = await ctrl.LoginAsync(new LoginRequest { UserName = "admin", Password = "pass" });

            var resp = GetOkValue(result);
            resp!.Code.Should().Be(ResponseCode.Success);
            resp.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task LoginAsync_UserNull_ReturnsBadRequest()
        {
            var (ctrl, _, userMock, _, _, _) = CreateController();
            userMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync((User?)null);

            var result = await ctrl.LoginAsync(new LoginRequest { UserName = "ghost", Password = "x" });

            var resp = GetBadValue(result);
            resp!.ErrorCode.Should().Be(ErrorCode.InvalidCredentials);
        }

        [Fact]
        public async Task LoginAsync_ServiceThrowsBusinessException_PropagatesException()
        {
            var (ctrl, _, userMock, _, _, _) = CreateController();
            userMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ThrowsAsync(new BusinessException(ErrorCode.UserNotFound));

            var act = async () => await ctrl.LoginAsync(new LoginRequest { UserName = "nobody" });

            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.ErrorCode == ErrorCode.UserNotFound);
        }

        [Fact]
        public async Task LoginAsync_LogsSuccessOnSuccess()
        {
            var (ctrl, jwtMock, userMock, _, _, logMock) = CreateController();
            var user = new User { Id = 1, UserName = "admin" };
            userMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(user);
            jwtMock.Setup(s => s.GenerateToken(It.IsAny<IEnumerable<Claim>>())).Returns("tok");

            await ctrl.LoginAsync(new LoginRequest { UserName = "admin", Password = "p" });

            logMock.Verify(l => l.AddAsync(It.Is<LoginLog>(log => log.Result == (int)LoginResult.Success), default), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_BusinessException_LogsFailure()
        {
            var (ctrl, _, userMock, _, _, logMock) = CreateController();
            userMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ThrowsAsync(new BusinessException(ErrorCode.InvalidCredentials));

            try { await ctrl.LoginAsync(new LoginRequest { UserName = "user" }); } catch { }

            logMock.Verify(l => l.AddAsync(It.Is<LoginLog>(log => log.Result == (int)LoginResult.Failed), default), Times.Once);
        }

        // ─── GetAccessCode ────────────────────────────────────────────────────

        [Fact]
        public async Task GetAccessCode_UserFound_ReturnsPermissions()
        {
            var (ctrl, _, userMock, currentUserMock, _, _) = CreateController();
            currentUserMock.Setup(c => c.Id).Returns(1);
            var role = new Role { Name = "Admin", Status = (int)DataStatus.Active };
            var perm = new Permission { Code = "user:get", Status = (int)DataStatus.Active };
            role.Permissions.Add(perm);
            var user = new User { Id = 1, UserName = "admin" };
            user.Roles.Add(role);
            userMock.Setup(s => s.GetOneAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
                .ReturnsAsync(user);

            var result = await ctrl.GetAccessCode();

            var resp = GetOkValue(result);
            resp!.Code.Should().Be(ResponseCode.Success);
        }

        [Fact]
        public async Task GetAccessCode_UserNotFound_ReturnsBadRequest()
        {
            var (ctrl, _, userMock, currentUserMock, _, _) = CreateController();
            currentUserMock.Setup(c => c.Id).Returns(1);
            userMock.Setup(s => s.GetOneAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
                .ReturnsAsync((User?)null);

            var result = await ctrl.GetAccessCode();

            var resp = GetBadValue(result);
            resp!.ErrorCode.Should().Be(ErrorCode.UserNotFound);
        }
    }
}
