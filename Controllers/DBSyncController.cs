using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("DBSync"),Authorize(Policy = "SuperAdmin")]
    public class DBSyncController(DataSyncService _dataSyncService) : ControllerBase
    {
        [HttpPost("SyncUsers")]
        public async Task<IActionResult> SyncUsers()
        {
            try
            {
                await _dataSyncService.SyncUsersAsync();
                return Ok("Users synchronized successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("SyncLogs")]
        public async Task<IActionResult> SyncLogs()
        {
            try
            {
                await _dataSyncService.SyncLogsAsync();
                return Ok("Logs synchronized successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}