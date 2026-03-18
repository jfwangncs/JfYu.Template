using System.ComponentModel.DataAnnotations;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Constants;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Role;
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
    }
}
