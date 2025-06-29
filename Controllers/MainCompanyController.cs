using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Dtos.MainCompanyDtos;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Services;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController, Route("MainCompany")]
    public class MainCompanyController(MainCompanyService _mainCompanyService) : ControllerBase
    {
        [HttpGet("GetAllMainCompanies")]
        public async Task<IActionResult> GetAllMainCompaniesAsync()
        {
            var companies = await _mainCompanyService.GetAllMainCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("GetMainCompanyById/{id}")]
        public async Task<IActionResult> GetMainCompanyByIdAsync(int id)
        {
            var company = await _mainCompanyService.GetMainCompanyByIdAsync(id);
            return company != null ? Ok(company) : NotFound();
        }
        [HttpPost("AddMainCompany")]
        public async Task<IActionResult> AddMainCompanyAsync([FromBody]MainCompanyAddDto mainCompanyDto)
        {
            if (mainCompanyDto == null || string.IsNullOrWhiteSpace(mainCompanyDto.Name) || mainCompanyDto.SpecialtyId <= 0)
            {
                return BadRequest("Invalid company data.");
            }

            var addedCompany = await _mainCompanyService.AddMainCompanyAsync(mainCompanyDto);
            return Ok(addedCompany);
        }
  
    }
}