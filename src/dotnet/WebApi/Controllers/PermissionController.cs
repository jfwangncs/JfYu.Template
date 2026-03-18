using Mapster;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApi.Attributes;
using WebApi.Constants;
using WebApi.Model;
using WebApi.Model.Permission;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Permission(PermissionCodes.Permission, PermissionType.Menu, parentCode: PermissionCodes.System)]
    public class PermissionController(IPermissionService permissionService) : CustomController
    {
        private readonly IPermissionService _permissionService = permissionService;

        [HttpGet]
        [Permission(PermissionCodes.PermissionGet)]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryRequest query)
        {
            var result = await _permissionService.GetPagedAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Permission(PermissionCodes.PermissionGet)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var permission = await _permissionService.GetOneAsync(q => q.Id.Equals(id));
            if (permission == null)
                return BadRequest(ErrorCode.PermissionNotFound);
            return Ok(permission.Adapt<PermissionResponse>());
        }

        [HttpPut("{id}")]
        [Permission(PermissionCodes.PermissionEdit)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody][Required] UpdatePermissionRequest request)
        {
            var permission = await _permissionService.GetOneAsync(q => q.Id.Equals(id));
            if (permission == null)
                return BadRequest(ErrorCode.PermissionNotFound);

            if (request.Name != null) permission.Name = request.Name;
            if (request.Description != null) permission.Description = request.Description;
            if (request.Icon != null) permission.Icon = request.Icon;
            if (request.Sort.HasValue) permission.Sort = request.Sort.Value;

            if (await _permissionService.UpdateAsync(permission) > 0)
                return Ok(permission.Adapt<PermissionResponse>());
            else
                return BadRequest(ErrorCode.OperationFailed);
        }

        [HttpPost("sync")]
        [Permission(PermissionCodes.PermissionSync)]
        public IActionResult Sync()
        {
            _permissionService.SyncAsync();
            return Ok();
        }
    }
}
