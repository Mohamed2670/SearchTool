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

        [HttpGet("searchByName")]
        public async Task<IActionResult> SearchByname(string name)
        {
            var items = await _drugService.SearchName(name);
            return Ok(items);
        }

        //get drug insurances by drug name
        [HttpGet("getDrugInsurances")]
        public async Task<IActionResult> GetDrugInsurances(string name)
        {
            var items = await _drugService.GetDrugInsurances(name);
            return Ok(items);
        }

        //get drug  ndc codes by drug name
        [HttpGet("getDrugNDCs")]
        public async Task<IActionResult> GetDrugNDCs(string name)
        {
            var items = await _drugService.GetDrugNDCs(name);
            return Ok(items);
        }
    }
}