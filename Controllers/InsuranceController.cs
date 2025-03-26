using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Services;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("Insurance")]
    public class InsuranceContorller(InsuranceService _insuranceService) : ControllerBase
    {
        [HttpGet("GetInsuranceDetails"), Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetInsuranceDetails([FromQuery] int id)
        {
            var item = await _insuranceService.GetInsuranceDetails(id);
            return Ok(item);
        }
        [HttpGet("GetAllRxGroups"), Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAllRxGroups()
        {
            var items = await _insuranceService.GetAllRxGroups();
            return Ok(items);
        }
        [HttpGet("GetAllRxGroupsByPcnId")]
        public async Task<IActionResult> GetAllRxGroupsByPcnId(int id)
        {
            var items = await _insuranceService.GetAllRxGroupsByPcnId(id);
            return Ok(items);
        }
        [HttpGet("GetAllPCNByBinId")]
        public async Task<IActionResult> GetAllPCNByBinId(int id)
        {
            var items = await _insuranceService.GetAllPCNByBinId(id);
            return Ok(items);
        }
        [HttpGet("GetAllBIN")]
        public async Task<IActionResult> GetAllBIN()
        {
            var items = await _insuranceService.GetAllBIN();
            return Ok(items);
        }
        [HttpGet("GetInsurancePCNDetails")]
        public async Task<IActionResult> GetInsurancePCNDetails(int id)
        {
            var item = await _insuranceService.GetInsurancePCNDetails(id);
            return Ok(item);
        }
        [HttpGet("GetInsuranceBINDetails")]
        public async Task<IActionResult> GetInsuranceBINDetails(int id)
        {
            var item = await _insuranceService.GetInsuranceBINDetails(id);
            return Ok(item);
        }
        [HttpGet("GetAllRxGroupsByBINId")]
        public async Task<IActionResult> GetAllRxGroupsByBINId(int id)
        {
            var items = await _insuranceService.GetAllRxGroupsByBINId(id);
            return Ok(items);
        }
        [HttpGet("GetAllPCNsByBINId")]
        public async Task<IActionResult> GetAllPCNsByBINId(int id)
        {
            var items = await _insuranceService.GetAllPCNsByBINId(id);
            return Ok(items);
        }
    }
}