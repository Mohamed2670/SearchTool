using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Authentication;
using SearchTool_ServerSide.Dtos.LogDtos;
using SearchTool_ServerSide.Services;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController, Route("Logs"), Authorize]
    public class LogsContorller(LogsService _logsService, UserAccessToken _userAccessToken) : ControllerBase
    {
        [HttpGet("GetLogs"), Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetLogs()
        {
            var tokenRead = _userAccessToken.tokenData();
            var items = await _logsService.GetAllLogs(int.Parse(tokenRead.UserId));

            return Ok(items);
        }
        [HttpGet("GetAllLogsToSharePoint"), AllowAnonymous]
        public async Task<IActionResult> GetAllLogsToSharePoint()
        {
            var items = await _logsService.GetAllLogsToSharePoint();
            return Ok(items);
        }
        [HttpPost("InsertAllLogsToDB"), AllowAnonymous]
        public async Task<IActionResult> InsertAllLogsToDB([FromBody] IEnumerable<AllLogsAddDto> allLogsAddDtos)
        {
            await _logsService.InsertAllLogsToDB(allLogsAddDtos);
            return Ok("All Logs Saved in DB :)");
        }
        

    }
}