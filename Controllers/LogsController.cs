using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Authentication;
using SearchTool_ServerSide.Services;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController, Route("Logs")]
    public class LogsContorller : ControllerBase
    {
        [HttpGet("GetLogs"),Authorize(Policy ="Admin")]
        public async Task<IActionResult> GetLogs(LogsService _logsService,UserAccessToken _userAccessToken)
        {
            var tokenRead = _userAccessToken.tokenData();
            var items = await _logsService.GetAllLogs(int.Parse(tokenRead.UserId));

            return Ok(items);
        }
    }
}