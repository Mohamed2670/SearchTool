using AutoMapper;
using SearchTool_ServerSide.Dtos.LogDtos;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Profiles
{
    public class LogProfile : Profile
    {
        public LogProfile()
        {
            CreateMap<Log, AllLogsAddDto>().ReverseMap();
        }
    }
}