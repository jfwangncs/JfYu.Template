using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Constants;
using WebApi.Controllers;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Services.Interfaces;

namespace WebApi.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _controller = new UserController(_mockUserService.Object, _mockCurrentUser.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOkWithUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new() { NickName = "Alice", Role = UserRoleEnum.Admin },
                new() { NickName = "Bob", Role = UserRoleEnum.General }
            };
            _mockUserService.Setup(x => x.GetListAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as BaseResponse<object>;
            response!.Code.Should().Be(ResponseCode.Success);
            response.Data.Should().Be(users);
            _mockUserService.Verify(x => x.GetListAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WithNoUsers_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            _mockUserService.Setup(x => x.GetListAsync()).ReturnsAsync([]);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as BaseResponse<object>;
            response!.Code.Should().Be(ResponseCode.Success);
            _mockUserService.Verify(x => x.GetListAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnOkWithUser()
        {
            // Arrange
            var user = new User { NickName = "Alice", Phone = "1234567890", Role = UserRoleEnum.Admin };
            _mockUserService
                .Setup(x => x.GetOneAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetByIdAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as BaseResponse<object>;
            response!.Code.Should().Be(ResponseCode.Success);
            response.Data.Should().Be(user);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            _mockUserService
                .Setup(x => x.GetOneAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetByIdAsync(999);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badResult = result as BadRequestObjectResult;
            var response = badResult!.Value as BaseResponse<object>;
            response!.Code.Should().Be(ResponseCode.Failed);
            response.ErrorCode.Should().Be(ErrorCode.ValidationError);
            response.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldCallServiceGetOneAsync()
        {
            // Arrange
            var user = new User { NickName = "TestUser" };
            _mockUserService
                .Setup(x => x.GetOneAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            await _controller.GetByIdAsync(42);

            // Assert
            _mockUserService.Verify(x => x.GetOneAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        }
    }
}
