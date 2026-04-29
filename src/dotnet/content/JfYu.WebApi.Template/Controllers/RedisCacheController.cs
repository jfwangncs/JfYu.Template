using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
//#if (EnableRBAC)
using JfYu.WebApi.Template.Attributes;
using JfYu.WebApi.Template.Constants;
//#endif
using JfYu.WebApi.Template.Model.RedisCache;
using JfYu.WebApi.Template.Services.Interfaces;

namespace JfYu.WebApi.Template.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //#if (EnableRBAC)
    [Permission(PermissionCodes.RedisCache, PermissionType.Menu, parentCode: PermissionCodes.System)]
    //#endif
    public class RedisCacheController(IRedisCacheService redisCacheService) : CustomController
    {
        private readonly IRedisCacheService _redisCacheService = redisCacheService;

        [HttpGet]
        //#if (EnableRBAC)
        [Permission(PermissionCodes.RedisCacheGet)]
        //#endif
        public async Task<IActionResult> GetAllAsync()
        {
            var items = await _redisCacheService.GetAllAsync();
            return Ok(items);
        }

        [HttpDelete]
        //#if (EnableRBAC)
        [Permission(PermissionCodes.RedisCacheDelete)]
        //#endif
        public async Task<IActionResult> DeleteAsync([FromBody][Required] DeleteRedisCacheRequest request)
        {
            if (request.Keys.Count == 0)
                return Ok();
            await _redisCacheService.DeleteAsync(request.Keys);
            return Ok();
        }
    }
}
