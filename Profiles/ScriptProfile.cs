using AutoMapper;
using SearchTool_ServerSide.Dtos;
using ServerSide.Models;

namespace SearchTool_ServerSide.Profiles
{
    public class ScriptProfile : Profile
    {
        public ScriptProfile()
        {
            CreateMap<AuditReadDto, Script>();
            CreateMap<Script, AuditReadDto>();
        }
    }
}