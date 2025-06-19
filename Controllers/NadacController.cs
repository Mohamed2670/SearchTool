using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Services;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("nadac")]
    public class NadacController(NadacService _nadacService) : ControllerBase
    {
        [HttpGet("UpateNadacPrices")]
        public async Task<IActionResult> UpateNadacPrices([FromQuery]string csvUrl)
        {
            if (string.IsNullOrEmpty(csvUrl))
            {
                return BadRequest("CSV URL cannot be null or empty.");
            }
            if(!csvUrl.StartsWith("https://download.medicaid.gov/data/") && !csvUrl.StartsWith("https://data.medicaid.gov/api"))
            {
                return BadRequest("Invalid CSV URL.");
            }
            try
            {
                await _nadacService.GetMatchingPricesAsync(csvUrl);
                return Ok("NADAC prices processed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}