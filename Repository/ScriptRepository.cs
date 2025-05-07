using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;
using ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class ScriptRepository : GenericRepository<Script>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public ScriptRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


    }
}