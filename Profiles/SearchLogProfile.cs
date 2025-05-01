using AutoMapper;
using SearchTool_ServerSide.Dtos.SearchLogDtos;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Profiles
{
    public class SearchLogProfile : Profile
    {
        public SearchLogProfile()
        {
            CreateMap<SearchLog, SearchLogAddDto>().ReverseMap();
        }

    }
}