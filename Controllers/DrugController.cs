using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Services;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("drug")]
    public class DrugController(DrugService _drugService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> SaveData()
        {
            await _drugService.Procces();
            return Ok();
        }
        [HttpGet("/temp2")]
        public async Task<IActionResult> temp2()
        {
            await _drugService.Procces2();
            return Ok();
        }
    }
}