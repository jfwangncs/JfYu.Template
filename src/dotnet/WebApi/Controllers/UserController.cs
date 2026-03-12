using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using System.ComponentModel.DataAnnotations;
using WebApi.Constants;
using WebApi.Entity;
using WebApi.Model.Request;
using WebApi.Model.Response;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController(IUserService userService, ICurrentUser currentUser) : CustomController
    {
        private readonly IUserService _userService = userService;
        private readonly ICurrentUser _currentUser = currentUser;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryRequest query)
        {
            var result = await _userService.GetPagedAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var user = await _userService.GetOneAsync(q => q.Id.Equals(id));
            if (user == null)
                return BadRequest(ErrorCode.UserNotFound);
            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody][Required] CreateUserRequest request)
        {
            var exist = await _userService.GetOneAsync(q => q.UserName == request.UserName);
            if (exist != null)
                return BadRequest(ErrorCode.DuplicateUser);

            var user = request.Adapt<User>();
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            await _userService.AddAsync(user);
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _userService.RemoveAsync(q => q.Id.Equals(id));
            if (result <= 0)
                return BadRequest(ErrorCode.UserNotFound);
            return Ok();
        }

        [HttpGet("info")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            var user = await _userService.GetOneAsync(q => q.Id.Equals(_currentUser.Id));
            if (user == null)
                return BadRequest(ErrorCode.UserNotFound);
            return Ok(user.Adapt<UserResponse>());
        }
    }
}
