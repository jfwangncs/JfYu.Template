using JfYu.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebApi.Constants;
using WebApi.Entity;
using WebApi.Model.Request;
using WebApi.Services;

namespace WebApi.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly ReadonlyDBContext<AppDbContext> _readonlyContext;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _readonlyContext = new ReadonlyDBContext<AppDbContext>(_dbContext);
            _userService = new UserService(_dbContext, _readonlyContext);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnAllUsers()
        {
            // Arrange
            _dbContext.Users.AddRange(
                new User { NickName = "Alice", Phone = "1234567890", Role = UserRoleEnum.Admin },
                new User { NickName = "Bob", Phone = "0987654321", Role = UserRoleEnum.General }
            );
            await _dbContext.SaveChangesAsync();

            // Act
            var users = await _userService.GetListAsync();

            // Assert
            users.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetListAsync_WithNoUsers_ShouldReturnEmptyList()
        {
            // Act
            var users = await _userService.GetListAsync();

            // Assert
            users.Should().BeEmpty();
        }

        [Fact]
        public async Task GetOneAsync_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                NickName = "Alice",
                Phone = "1234567890",
                Role = UserRoleEnum.Admin,
                OpenId = "open-id-123",
                Gender = GenderEnum.Female,
                City = "Shanghai"
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetOneAsync(q => q.Id.Equals(user.Id));

            // Assert
            result.Should().NotBeNull();
            result!.NickName.Should().Be("Alice");
            result.Phone.Should().Be("1234567890");
            result.Role.Should().Be(UserRoleEnum.Admin);
            result.OpenId.Should().Be("open-id-123");
            result.Gender.Should().Be(GenderEnum.Female);
            result.City.Should().Be("Shanghai");
        }

        [Fact]
        public async Task GetOneAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _userService.GetOneAsync(q => q.Id.Equals(999));

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnUsersWithAllFields()
        {
            // Arrange
            var user = new User
            {
                NickName = "TestUser",
                RealName = "Test Real Name",
                Phone = "1234567890",
                Avatar = "https://example.com/avatar.jpg",
                Gender = GenderEnum.Male,
                Province = "Guangdong",
                City = "Shenzhen",
                Country = "China",
                Address = "123 Test Street",
                Role = UserRoleEnum.General,
                OpenId = "open-id-456",
                UnionId = "union-id-789",
                SessionKey = "session-key-abc"
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var users = await _userService.GetListAsync();

            // Assert
            users.Should().HaveCount(1);
            var result = users[0];
            result.NickName.Should().Be("TestUser");
            result.RealName.Should().Be("Test Real Name");
            result.Phone.Should().Be("1234567890");
            result.Avatar.Should().Be("https://example.com/avatar.jpg");
            result.Gender.Should().Be(GenderEnum.Male);
            result.Province.Should().Be("Guangdong");
            result.City.Should().Be("Shenzhen");
            result.Country.Should().Be("China");
            result.Address.Should().Be("123 Test Street");
            result.Role.Should().Be(UserRoleEnum.General);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnUser()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin!@#p");
            var user = new User
            {
                UserName = "admin",
                Password = hashedPassword,
                Role = UserRoleEnum.Admin
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var request = new LoginRequest
            {
                UserName = "admin",
                Password = "admin!@#p",
                Platform = PlatformEnum.Web
            };

            // Act
            var result = await _userService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.UserName.Should().Be("admin");
            result.Role.Should().Be(UserRoleEnum.Admin);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidUsername_ShouldReturnNull()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin!@#p");
            _dbContext.Users.Add(new User
            {
                UserName = "admin",
                Password = hashedPassword,
                Role = UserRoleEnum.Admin
            });
            await _dbContext.SaveChangesAsync();

            var request = new LoginRequest
            {
                UserName = "wronguser",
                Password = "admin!@#p",
                Platform = PlatformEnum.Web
            };

            // Act
            var result = await _userService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin!@#p");
            _dbContext.Users.Add(new User
            {
                UserName = "admin",
                Password = hashedPassword,
                Role = UserRoleEnum.Admin
            });
            await _dbContext.SaveChangesAsync();

            var request = new LoginRequest
            {
                UserName = "admin",
                Password = "wrongpassword",
                Platform = PlatformEnum.Web
            };

            // Act
            var result = await _userService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            var request = new LoginRequest
            {
                UserName = "nonexistent",
                Password = "anypassword",
                Platform = PlatformEnum.Web
            };

            // Act
            var result = await _userService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldUpdateLastLoginTime()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin!@#p");
            var originalTime = new DateTime(2020, 1, 1);
            var user = new User
            {
                UserName = "admin",
                Password = hashedPassword,
                Role = UserRoleEnum.Admin,
                LastLoginTime = originalTime
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var request = new LoginRequest
            {
                UserName = "admin",
                Password = "admin!@#p",
                Platform = PlatformEnum.Web
            };

            // Act
            var result = await _userService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.LastLoginTime.Should().BeAfter(originalTime);
        }
    }
}
