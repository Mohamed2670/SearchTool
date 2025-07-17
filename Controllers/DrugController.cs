using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Authentication;
using SearchTool_ServerSide.Dtos.ScritpsDto;
using SearchTool_ServerSide.Models;
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
        [HttpGet("/GetAllDrugs")]
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
        [HttpGet("GetEPCMOAClassesByDrugId")]
        public async Task<IActionResult> GetEPCMOAClassesByDrugId([FromQuery] int drugId)
        {
            var items = await _drugService.GetEPCMOAClassesByDrugId(drugId);
            return Ok(items);
        }
        [HttpGet("searchByName")]
        public async Task<IActionResult> SearchByname([FromQuery] string name, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            // Console.WriteLine("Name: " + name+" PageNumber: " + pageNumber + " PageSize: " + pageSize);
            var items = await _drugService.SearchName(name, pageNumber, pageSize);
            return Ok(items);
        }
        [HttpGet("GetClassesByName")]
        public async Task<IActionResult> GetClassesByName([FromQuery] string name, [FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string classVersion)
        {
            // Console.WriteLine("Name: " + name+" PageNumber: " + pageNumber + " PageSize: " + pageSize);
            var items = await _drugService.GetClassesByName(name, classVersion, pageNumber, pageSize);
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
        [HttpGet("GetClassesByDrugId")]
        public async Task<IActionResult> GetClassesByDrugId([FromQuery] int drugId)
        {
            var items = await _drugService.GetClassesByDrugId(drugId);
            return Ok(items);
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

        [HttpGet("GetAllDrugsEPCMOA")]
        public async Task<IActionResult> GetAllDrugsEPCMOA([FromQuery] int drugId, [FromQuery] int pageSize = 1000, [FromQuery] int pageNumber = 1)
        {
            var items = await _drugService.GetAllDrugsEPCMOA(drugId, pageSize, pageNumber);
            return Ok(items);
        }
        [HttpGet("GetAllDrugs")]
        public async Task<IActionResult> GetAllDrugs([FromQuery] int classId)
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
        // [HttpGet("oneway")]
        // public async Task<IActionResult> oneway()
        // {
        //     await _drugService.oneway();
        //     return Ok(true);
        // }
        [HttpGet("GetInsuranceByNdc")]
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
        [HttpGet("ImportInsurancesFromCsvAsync"),AllowAnonymous]
        public async Task<IActionResult> ImportInsurancesFromCsvAsync()
        {
            await _drugService.ImportInsurancesFromCsvAsync();
            return Ok();
        }

        [HttpGet("GetDrugsByInsurance")]
        public async Task<IActionResult> GetDrugsByInsurance([FromQuery] int insuranceId, [FromQuery] string drug)
        {
            var items = await _drugService.GetDrugsByInsurance(insuranceId, drug);
            return Ok(items);
        }
        [HttpGet("GetDrugsByInsuranceName")]
        public async Task<IActionResult> GetDrugsByInsurance([FromQuery] string insurance)
        {
            var items = await _drugService.GetDrugsByInsurance(insurance);
            return Ok(items);
        }
        [HttpGet("GetDrugsByInsuranceNamePagintated")]
        public async Task<IActionResult> GetDrugsByInsuranceNamePagintated([FromQuery] string insurance, [FromQuery] string drugName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var items = await _drugService.GetDrugsByInsuranceNamePaginated(insurance, drugName, pageSize, pageNumber);
            return Ok(items);
        }
        [HttpGet("GetDrugsByPCNPagintated")]
        public async Task<IActionResult> GetDrugsByPCNPagintated([FromQuery] string insurance, [FromQuery] string drugName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var items = await _drugService.GetDrugsByPCNPaginated(insurance, drugName, pageSize, pageNumber);
            return Ok(items);
        }
        [HttpGet("GetDrugsByBINPagintated")]
        public async Task<IActionResult> GetDrugsByBINPagintated([FromQuery] string insurance, [FromQuery] string drugName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var items = await _drugService.GetDrugsByBINPaginated(insurance, drugName, pageSize, pageNumber);
            return Ok(items);
        }
        [HttpGet("GetDrugsByInsuranceNameDrugName")]
        public async Task<IActionResult> GetDrugsByInsuranceNameDrugName(
            [FromQuery] string insurance,
            [FromQuery] string drugName,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var items = await _drugService.GetDrugsByInsuranceNameDrugName(insurance, drugName, pageSize, pageNumber);
            return Ok(items);
        }
        [HttpGet("GetDrugsByPCN")]
        public async Task<IActionResult> GetDrugsByPCN([FromQuery] string pcn)
        {
            var items = await _drugService.GetDrugsByPCN(pcn);
            return Ok(items);
        }
        [HttpGet("GetDrugsByBIN")]
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
        public async Task<IActionResult> GetAllLatestScriptsPaginated([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string classVersion = "ClassV1")
        {
            var items = await _drugService.GetAllLatestScriptsPaginated(pageNumber, pageSize, classVersion);
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

        [HttpGet("AddMediCare"),AllowAnonymous]
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

        [HttpGet("GetDrugClassesByInsuranceNamePagintated")]
        public async Task<IActionResult> GetDrugClassesByInsuranceNamePagintated([FromQuery] string insurance, [FromQuery] string drugClassName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string classVersion = "ClassV1")
        {
            var items = await _drugService.GetDrugClassesByInsuranceNamePagintated(insurance, drugClassName, pageSize, pageNumber, classVersion);
            return Ok(items);
        }
        [HttpGet("GetDrugClassesByPCNPagintated")]
        public async Task<IActionResult> GetDrugClassesByPCNPagintated([FromQuery] string insurance, [FromQuery] string drugClassName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string classVersion = "ClassV1")
        {
            var items = await _drugService.GetDrugClassesByPCNPagintated(insurance, drugClassName, pageSize, pageNumber, classVersion);
            return Ok(items);
        }
        [HttpGet("GetDrugClassesByBINPagintated")]
        public async Task<IActionResult> GetDrugClassesByBINPagintated([FromQuery] string insurance, [FromQuery] string drugClassName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string classVersion = "ClassV1")
        {
            var items = await _drugService.GetDrugClassesByBINPagintated(insurance, drugClassName, pageSize, pageNumber, classVersion);
            return Ok(items);
        }
        [HttpGet("GetDrugsByClassId")]
        public async Task<IActionResult> GetDrugsByClassId([FromQuery] int classId, [FromQuery] string classType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var items = await _drugService.GetDrugsByClassId(classId, classType, pageSize, pageNumber);
            return Ok(items);
        }
        

    }
}