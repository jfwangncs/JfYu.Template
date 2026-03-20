using Mapster;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApi.Attributes;
using WebApi.Constants;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Response;
using WebApi.Model.Role;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Permission(PermissionCodes.Role, PermissionType.Menu, parentCode: PermissionCodes.System)]
    public class RoleController(IRoleService roleService) : CustomController
    {
        private readonly IRoleService _roleService = roleService;

        [HttpGet]
        [Permission(PermissionCodes.RoleGet)]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryRequest query)
        {
            var result = await _roleService.GetPagedAsync(query);
            return Ok(result.Adapt<PagedResult<RoleResponse>>());
        }

        [HttpGet("{id}")]
        [Permission(PermissionCodes.RoleGet)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var role = await _roleService.GetOneAsync(q => q.Id.Equals(id));
            if (role == null)
                return BadRequest(ErrorCode.RoleNotFound);
            return Ok(role);
        }

        [HttpPost]
        [Permission(PermissionCodes.RoleAdd)]
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
        [Permission(PermissionCodes.RoleEdit)]
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
    }
}
