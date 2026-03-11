using System.ComponentModel.DataAnnotations;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Constants;
using WebApi.Entity;
using WebApi.Model.Request;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RoleController(IRoleService roleService) : CustomController
    {
        private readonly IRoleService _roleService = roleService;

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryRequest query)
        {
            var result = await _roleService.GetPagedAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var role = await _roleService.GetOneAsync(q => q.Id.Equals(id));
            if (role == null)
                return BadRequest(ErrorCode.RoleNotFound);
            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody][Required] CreateRoleRequest request)
        {
            var exist = await _roleService.GetOneAsync(q => q.Name.Equals(request.Name));
            if (exist != null)
                return BadRequest(ErrorCode.DuplicateRole);
            var role = request.Adapt<Role>();
            await _roleService.AddAsync(role);
            return Ok(role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody][Required] UpdateRoleRequest request)
        {
            var role = await _roleService.GetOneAsync(q => q.Id.Equals(id));
            if (role == null)
                return BadRequest(ErrorCode.RoleNotFound);

            var exist = await _roleService.GetOneAsync(q => q.Name.Equals(request.Name) && q.Id != id);
            if (exist != null)
                return BadRequest(ErrorCode.DuplicateRole);

            if (await _roleService.UpdateAsync(request.Adapt(role)) > 0)
                return Ok(role);
            else
                return BadRequest(ErrorCode.OperationFailed);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _roleService.RemoveAsync(q => q.Id.Equals(id));
            if (result <= 0)
                return BadRequest(ErrorCode.ValidationError, "Role not found.");
            return Ok();
        }

        [HttpPost("{roleId}/permissions")]
        public async Task<IActionResult> AssignPermissionsAsync(int roleId, [FromBody][Required] AssignRolePermissionRequest request)
        {
            request.RoleId = roleId;
            await _roleService.AssignPermissionsAsync(request);
            return Ok();
        }

        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetRolePermissionsAsync(int roleId)
        {
            var permissions = await _roleService.GetRolePermissionsAsync(roleId);
            return Ok(permissions);
        }

        [HttpPost("assign-user")]
        public async Task<IActionResult> AssignUserRolesAsync([FromBody][Required] AssignUserRoleRequest request)
        {
            await _roleService.AssignUserRolesAsync(request);
            return Ok();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRolesAsync(int userId)
        {
            var roles = await _roleService.GetUserRolesAsync(userId);
            return Ok(roles);
        }
    }
}
