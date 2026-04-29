using Mapster;
using Microsoft.AspNetCore.Mvc;
using JfYu.WebApi.Template.Attributes;
using JfYu.WebApi.Template.Constants;
using JfYu.WebApi.Template.Model;
using JfYu.WebApi.Template.Model.LoginLog;
using JfYu.WebApi.Template.Services.Interfaces;

namespace JfYu.WebApi.Template.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Permission(PermissionCodes.LoginLog, PermissionType.Menu, parentCode: PermissionCodes.System)]
    public class LoginLogController(ILoginLogService loginLogService) : CustomController
    {
        private readonly ILoginLogService _loginLogService = loginLogService;

        [HttpGet]
        [Permission(PermissionCodes.LoginLogGet)]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryLoginLogRequest query)
        {
            var result = await _loginLogService.GetPagedAsync(query);
            return Ok(result.Adapt<PagedResult<LoginLogResponse>>());
        }
    }
}