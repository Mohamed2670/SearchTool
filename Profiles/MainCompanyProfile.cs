using AutoMapper;
using SearchTool_ServerSide.Dtos.MainCompanyDtos;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Profiles
{
    public class MainCompanyProfile : Profile
    {
        public MainCompanyProfile()
        {
            CreateMap<MainCompany, MainCompanyReadDto>().ReverseMap();
            CreateMap<MainCompany, MainCompanyAddDto>().ReverseMap();
        }
    }
}