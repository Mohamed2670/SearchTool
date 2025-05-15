using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Authentication;
using SearchTool_ServerSide.Dtos.ScritpsDto;
using SearchTool_ServerSide.Services;
using ServerSide.Models;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController, Route("drug"), Authorize, Authorize(Policy = "Pharmacist")]
    public class DrugController(DrugService _drugService, UserAccessToken userAccessToken) : ControllerBase
    {
        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> SaveData()
        {
            await _drugService.Procces();
            return Ok();
        }
        [HttpGet("/temp2"), AllowAnonymous]
        public async Task<IActionResult> temp2()
        {
            await _drugService.Procces2();
            return Ok();
        }
        [HttpGet("/GetAllDrugs"), AllowAnonymous]
        public async Task<IActionResult> GetAllDrugs()
        {
            var items = await _drugService.GetAll();
            return Ok(items);
        }

        [HttpGet("SearchByNdc")]
        public async Task<IActionResult> SearchByNdc([FromQuery] string ndc)
        {
            var item = await _drugService.SearchByNdc(ndc);
            return Ok(item);
        }

        [HttpGet("searchByName")]
        public async Task<IActionResult> SearchByname([FromQuery] string name, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            // Console.WriteLine("Name: " + name+" PageNumber: " + pageNumber + " PageSize: " + pageSize);
            var items = await _drugService.SearchName(name, pageNumber, pageSize);
            return Ok(items);
        }

        [HttpGet("SearchByIdNdc")]
        public async Task<IActionResult> SearchByIdNdc([FromQuery] int id, [FromQuery] string ndc)
        {
            var item = await _drugService.SearchByIdNdc(id, ndc);
            return Ok(item);
        }

        // //get drug insurances by drug name
        // [HttpGet("getDrugInsurances")]
        // public async Task<IActionResult> GetDrugInsurances(string name)
        // {
        //     var items = await _drugService.GetDrugInsurances(name);
        //     return Ok(items);
        // }

        //get drug  ndc codes by drug name
        [HttpGet("getDrugNDCs")]
        public async Task<IActionResult> GetDrugNDCs(string name)
        {
            var items = await _drugService.GetDrugNDCs(name);
            return Ok(items);
        }
        [HttpGet("GetByslections")]
        public async Task<IActionResult> GetBySelection([FromQuery] string name, [FromQuery] string ndc, [FromQuery] string insuranceName)
        {
            var item = await _drugService.GetBySelection(name, ndc, insuranceName);
            return Ok(item);
        }
        [HttpGet("GetAltrantives"), AllowAnonymous]
        public async Task<IActionResult> GetAltrantives([FromQuery] string className, [FromQuery] int insuranceId)
        {
            var items = await _drugService.GetAltrantives(className, insuranceId);
            return Ok(items);
        }
        [HttpGet("GetDetails")]
        public async Task<IActionResult> GetDetails([FromQuery] string ndc, [FromQuery] int insuranceId)
        {
            var items = await _drugService.GetDetails(ndc, insuranceId);
            return Ok(items);
        }
        // [HttpGet("getDrugNDCsByNameInsuance")]
        // public async Task<IActionResult> getDrugNDCsByNameInsuance([FromQuery] string drugName, [FromQuery] int insurnaceId)
        // {
        //     var items = await _drugService.getDrugNDCsByNameInsuance(drugName, insurnaceId);
        //     return Ok(items);
        // }
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
        [HttpGet("GetDrugsByClassBranch")]
        public async Task<IActionResult> GetDrugsByClassBranch([FromQuery] int classId, [FromQuery] int branchId)
        {
            var items = await _drugService.GetDrugsByClassBranch(classId, branchId);
            return Ok(items);
        }

        [HttpGet("GetAllLatest")]
        public async Task<IActionResult> GetAllLatest()
        {
            var items = await _drugService.GetAllLatest();
            return Ok(items);
        }

        [HttpGet("GetAllLatestScripts"), AllowAnonymous]
        public async Task<IActionResult> GetAllLatestScripts()
        {
            var items = await _drugService.GetAllLatestScripts();
            return Ok(items);
        }
        [HttpGet("GetAllDrugs"), AllowAnonymous]
        public async Task<IActionResult> GetAllDrugs([FromQuery] int classId)
        {
            var items = await _drugService.GetAllDrugs(classId);
            return Ok(items);
        }
        [HttpGet("GetAllDrugsV2Insu"), AllowAnonymous]
        public async Task<IActionResult> GetAllDrugsV2Insu([FromQuery] int classId)
        {
            var items = await _drugService.GetAllDrugsV2Insu(classId);
            return Ok(items);
        }
        [HttpGet("GetAllDrugsV2"), AllowAnonymous]
        public async Task<IActionResult> GetAllDrugsV2([FromQuery] int classId)
        {
            var items = await _drugService.GetAllDrugsV2(classId);
            return Ok(items);
        }
        [HttpGet("GetDrugById"), AllowAnonymous]
        public async Task<IActionResult> GetDrugById([FromQuery] int id)
        {
            var item = await _drugService.GetDrugById(id);
            return Ok(item);
        }
        // [HttpGet("oneway")]
        // public async Task<IActionResult> oneway()
        // {
        //     await _drugService.oneway();
        //     return Ok(true);
        // }
        [HttpGet("GetInsuranceByNdc"), AllowAnonymous]
        public async Task<IActionResult> GetInsuranceByNdc([FromQuery] string ndc)
        {
            var items = await _drugService.GetInsuranceByNdc(ndc);
            return Ok(items);
        }
        [HttpGet("GetScriptByScriptCode"), Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetScriptByScriptCode([FromQuery] string scriptCode)
        {
            var script = await _drugService.GetScriptAsync(scriptCode);
            if (script == null)
            {
                return BadRequest("Invald script code");
            }
            if (!userAccessToken.IsAuthenticated(script.BranchId))
            {
                return Unauthorized("Not in the same branch");
            }
            var items = await _drugService.GetScriptByScriptCode(scriptCode);
            return Ok(items);
        }
        [HttpGet("ImportInsurancesFromCsvAsync"), AllowAnonymous]
        public async Task<IActionResult> ImportInsurancesFromCsvAsync()
        {
            await _drugService.ImportInsurancesFromCsvAsync();
            return Ok();
        }

        [HttpGet("GetAlternativesByClassIdBranchId")]
        public async Task<IActionResult> GetAlternativesByClassIdBranchId([FromQuery] int classId)
        {
            var userData = userAccessToken.tokenData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
            {
                return Unauthorized("Invalid or missing token data");
            }

            if (!int.TryParse(userData.BranchId, out int branchId))
            {
                return BadRequest("Invalid user ID format");
            }
            var items = await _drugService.GetAlternativesByClassIdBranchId(classId, branchId);
            return Ok(items);
        }
        [HttpGet("GetDrugsByInsurance")]
        public async Task<IActionResult> GetDrugsByInsurance([FromQuery] int insuranceId, [FromQuery] string drug)
        {
            var items = await _drugService.GetDrugsByInsurance(insuranceId, drug);
            return Ok(items);
        }
        [HttpGet("GetDrugsByInsuranceName"), AllowAnonymous]
        public async Task<IActionResult> GetDrugsByInsurance([FromQuery] string insurance)
        {
            var items = await _drugService.GetDrugsByInsurance(insurance);
            return Ok(items);
        }
        [HttpGet("GetDrugsByPCN"), AllowAnonymous]
        public async Task<IActionResult> GetDrugsByPCN([FromQuery] string pcn)
        {
            var items = await _drugService.GetDrugsByPCN(pcn);
            return Ok(items);
        }
        [HttpGet("GetDrugsByBIN"), AllowAnonymous]
        public async Task<IActionResult> GetDrugsByBIN([FromQuery] string bin)
        {
            var items = await _drugService.GetDrugsByBIN(bin);
            return Ok(items);
        }
        [HttpGet("GetInsurances")]
        public async Task<IActionResult> GetInsurances([FromQuery] string insurance)
        {
            var items = await _drugService.GetInsurances(insurance);
            return Ok(items);
        }
        [HttpGet("GetInsurancesBinsByName")]
        public async Task<IActionResult> GetInsurancesBinsByName([FromQuery] string bin)
        {
            var items = await _drugService.GetInsurancesBinsByName(bin);
            return Ok(items);
        }
        [HttpGet("GetInsurancesPcnByBinId")]
        public async Task<IActionResult> GetInsurancesPcnByBinId([FromQuery] int binId)
        {
            var items = await _drugService.GetInsurancesPcnByBinId(binId);
            return Ok(items);
        }
        [HttpGet("GetInsurancesRxByPcnId")]
        public async Task<IActionResult> GetInsurancesRxByPcnId([FromQuery] int pcnId)
        {
            var items = await _drugService.GetInsurancesRxByPcnId(pcnId);
            return Ok(items);
        }
        [HttpGet("GetAllLatestScriptsPaginated"), Authorize(Policy = "Admin"), Authorize]
        public async Task<IActionResult> GetAllLatestScriptsPaginated([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var items = await _drugService.GetAllLatestScriptsPaginated(pageNumber, pageSize);
            return Ok(items);
        }
        [HttpGet("GetAllLatestScriptsPaginatedv2"), Authorize]
        public async Task<IActionResult> GetAllLatestScriptsPaginatedv2([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var items = await _drugService.GetAllLatestScriptsPaginated(pageNumber, pageSize);
            return Ok(items);
        }
        [HttpGet("GetLatestScriptsByMonthYear"), Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetLatestScriptsByMonthYear([FromQuery] int month, [FromQuery] int year)
        {
            var items = await _drugService.GetLatestScriptsByMonthYear(month, year);
            return Ok(items);
        }
        [HttpPost("AddScritps"), Authorize(Policy = "Admin")]
        public async Task<IActionResult> AddScritps(ICollection<ScriptAddDto> scriptAddDtos)
        {
            Console.WriteLine("Hello : ");
            await _drugService.AddScripts(scriptAddDtos);
            return Ok("Items Added Succesfuly");
        }
        [HttpGet("GetBestAlternativeByNDCRxGroupId"), AllowAnonymous]
        public async Task<IActionResult> GetBestAlternativeByNDCRxGroupId([FromQuery] int classId, [FromQuery] int rxGroupId)
        {
            var items = await _drugService.GetBestAlternativeByNDCRxGroupId(classId, rxGroupId);
            if (items == null)
            {
                return NotFound("No alternatives found for the given classId and rxGroupId.");
            }
            return Ok(items);
        }
        [HttpGet("AddMediCare"), AllowAnonymous]
        public async Task<IActionResult> AddMediCare()
        {
            await _drugService.AddMediCare();
            return Ok("Items Added Succesfuly");
        }
        [HttpGet("GetAllMediDrugs"), AllowAnonymous]
        public async Task<IActionResult> GetAllMediDrugs([FromQuery] int classId)
        {
            var items = await _drugService.GetAllMediDrugs(classId);
            return Ok(items);
        }

    }
}