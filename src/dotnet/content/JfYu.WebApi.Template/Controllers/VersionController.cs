using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace JfYu.WebApi.Template.Controllers
{
    [ApiVersion("2.0")]
    public class VersionController : CustomController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("V2");
        }
    }
}
