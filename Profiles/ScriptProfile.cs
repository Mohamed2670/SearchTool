using AutoMapper;
using SearchTool_ServerSide.Dtos.DrugDtos;
using ServerSide.Models;

namespace SearchTool_ServerSide.Profiles
{
    public class ScriptProfile : Profile
    {
        public ScriptProfile()
        {
            CreateMap<ScriptAddDto, Script>();
            CreateMap<Script, ScriptAddDto>();
        }
    }
}