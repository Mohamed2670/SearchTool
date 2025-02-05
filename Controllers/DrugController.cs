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
        [HttpGet("SearchByNdc")]
        public async Task<IActionResult> SearchByNdc([FromQuery] string ndc)
        {
            var item = await _drugService.SearchByNdc(ndc);
            return Ok(item);
        }

        [HttpGet("searchByName")]
        public async Task<IActionResult> SearchByname(string name)
        {
            var items = await _drugService.SearchName(name);
            return Ok(items);
        }
        [HttpGet("SearchByIdNdc")]
        public async Task<IActionResult> SearchByIdNdc([FromQuery] int id ,[FromQuery] string ndc)
        {
            var item = await _drugService.SearchByIdNdc(id,ndc);
            return Ok(item);
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
        [HttpGet("GetByslections")]
        public async Task<IActionResult> GetBySelection([FromQuery]string name,[FromQuery] string ndc,[FromQuery]string insuranceName)
        {
            var item = await _drugService.GetBySelection(name, ndc, insuranceName);
            return Ok(item);
        }
        [HttpGet("GetAltrantives")]
        public async Task<IActionResult> GetAltrantives([FromQuery]string className,[FromQuery] int insuranceId)
        {
            var items = await _drugService.GetAltrantives(className, insuranceId);
            return Ok(items);
        }
        [HttpGet("GetDetails")]
        public async Task<IActionResult> GetDetails([FromQuery]string ndc,[FromQuery] int insuranceId)
        {
            var items = await _drugService.GetDetails(ndc, insuranceId);
            return Ok(items);
        }
        [HttpGet("getDrugNDCsByNameInsuance")]
        public async Task<IActionResult> getDrugNDCsByNameInsuance([FromQuery]string drugName,[FromQuery] int insurnaceId)
        {
            var items = await _drugService.getDrugNDCsByNameInsuance(drugName, insurnaceId);
            return Ok(items);
        }
        [HttpGet("GetClassById")]
        public async Task<IActionResult> getClassbyId([FromQuery] int id)
        {
            var item = await _drugService.getClassbyId(id);
            return Ok(item);
        }
        [HttpGet("GetDrugsByClass")]
        public async Task<IActionResult> GetDrugsByClass([FromQuery] int classId)
        {
            var items = await _drugService.GetDrugsByClass(classId);
            return Ok(items);
        }
        [HttpGet("GetAllLatest")]
        public async Task <IActionResult> GetAllLatest()
        {
            var items = await _drugService.GetAllLatest();
            return Ok(items);
        }
         [HttpGet("GetAllDrugs")]
        public async Task <IActionResult> GetAllDrugs([FromQuery]int classId)
        {
            var items = await _drugService.GetAllDrugs(classId);
            return Ok(items);
        }
        [HttpGet("GetDrugById")]
        public async Task<IActionResult> GetDrugById([FromQuery] int id)
        {
            var item = await _drugService.GetDrugById(id);
            return Ok(item);
        }
        [HttpGet("oneway")]
        public async Task<IActionResult> oneway()
        {
            await _drugService.oneway();
            return Ok(true);
        }
    }
}