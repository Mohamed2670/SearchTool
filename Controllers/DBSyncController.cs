using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("DBSync"), Authorize(Policy = "SuperAdmin")]
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
        [HttpPost("SyncUserDataCsv"), AllowAnonymous]
        public async Task<IActionResult> SyncUserDataCsv(string filePath = "Users.csv")
        {
            try
            {
                await _dataSyncService.SyncUserDataCsv(filePath);
                return Ok("User data synchronized successfully from CSV.");
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
        [HttpPost("SyncLogsCsv"), AllowAnonymous]
        public async Task<IActionResult> SyncLogsCsv(string filePath)
        {
            try
            {
                await _dataSyncService.ImportLogsFromCsvWithoutIdAsync(filePath);
                return Ok("Logs synchronized successfully from CSV.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("SyncLogsByExcel"), AllowAnonymous]
        public async Task<IActionResult> SyncLogsByExcel()
        {
            try
            {
                var csvBytes = await _dataSyncService.GetLogsCsvAsync();
                return File(csvBytes, "text/csv", "logs.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("SyncUserByExcel"), AllowAnonymous]
        public async Task<IActionResult> SyncUserByExcel()
        {
            try
            {
                var csvBytes = await _dataSyncService.SyncUserByExcel();
                return File(csvBytes, "text/csv", "Users.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}