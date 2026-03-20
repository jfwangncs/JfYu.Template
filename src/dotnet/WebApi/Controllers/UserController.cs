using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApi.Attributes;
using WebApi.Constants;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Request;
using WebApi.Model.Response;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Permission(PermissionCodes.User, PermissionType.Menu, parentCode: PermissionCodes.System)]
    public class UserController(IUserService userService, ICurrentUser currentUser, IPermissionService permissionService) : CustomController
    {
        private readonly IUserService _userService = userService;
        private readonly ICurrentUser _currentUser = currentUser;
        private readonly IPermissionService _permissionService = permissionService;

        [HttpGet]
        [Permission(PermissionCodes.UserGet)]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryRequest query)
        {
            var result = await _userService.GetPagedAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Permission(PermissionCodes.UserGet)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var user = await _userService.GetOneAsync(q => q.Id.Equals(id));
            if (user == null)
                return BadRequest(ErrorCode.UserNotFound);
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Permission(PermissionCodes.UserEdit)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody][Required] UpdateUserRequest request)
        {
            var user = await _userService.GetOneAsync(q => q.Id.Equals(id));
            if (user == null)
                return BadRequest(ErrorCode.UserNotFound);

            if (await _userService.UpdateAsync(request.Adapt(user)) > 0)
                return Ok(user.Adapt<UserResponse>());
            else
                return BadRequest(ErrorCode.OperationFailed);
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
