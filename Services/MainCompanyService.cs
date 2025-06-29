using AutoMapper;
using SearchTool_ServerSide.Dtos.MainCompanyDtos;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;

namespace SearchTool_ServerSide.Services
{
    public class MainCompanyService(MainCompanyRepository _mainCompanyService, IMapper _mapper)
    {
        public async Task<IEnumerable<MainCompanyReadDto>> GetAllMainCompaniesAsync()
        {
            var companies = await _mainCompanyService.GetAllMainCompaniesAsync();
            return _mapper.Map<IEnumerable<MainCompanyReadDto>>(companies);
        }
        public async Task<MainCompanyReadDto?> GetMainCompanyByIdAsync(int id)
        {
            var company = await _mainCompanyService.GetMainCompanyByIdAsync(id);
            return _mapper.Map<MainCompanyReadDto>(company);
        }
        public async Task<MainCompany?> AddMainCompanyAsync(MainCompanyAddDto mainCompanyDto)
        {
            if (mainCompanyDto == null || string.IsNullOrWhiteSpace(mainCompanyDto.Name) || mainCompanyDto.SpecialtyId <= 0)
            {
                return null; // Invalid data
            }

            var mainCompany = _mapper.Map<MainCompany>(mainCompanyDto);
            if (mainCompany == null)
            {
                return null; 
            }
            return await _mainCompanyService.AddMainCompanyAsync(mainCompany);

        }
    }
}