using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("sharePoint")]
    [Authorize, Authorize(Policy = "Admin")]
    public class SharePointController : ControllerBase
    {
        [HttpGet("token-test")]
        public IActionResult GetTokenTest()
        {
            return Ok("Token  is valid and working!");
        }
    }
}