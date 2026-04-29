using Mapster;
using Microsoft.AspNetCore.Mvc;
using JfYu.WebApi.Template.Attributes;
using JfYu.WebApi.Template.Constants;
using JfYu.WebApi.Template.Model;
using JfYu.WebApi.Template.Model.AuditLog;
using JfYu.WebApi.Template.Services.Interfaces;

namespace JfYu.WebApi.Template.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Permission(PermissionCodes.AuditLog, PermissionType.Menu, parentCode: PermissionCodes.System)]
    public class AuditLogController(IAuditLogService auditLogService) : CustomController
    {
        private readonly IAuditLogService _auditLogService = auditLogService;

        [HttpGet]
        [Permission(PermissionCodes.AuditLogGet)]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryAuditLogRequest query)
        {
            var result = await _auditLogService.GetPagedAsync(query);
            return Ok(result.Adapt<PagedResult<AuditLogResponse>>());
        }
    }
}
