using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class DrugClassRepository : GenericRepository<DrugClass>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public DrugClassRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


    }
}