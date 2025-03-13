using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class DrugBranchRepository : GenericRepository<DrugBranch>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public DrugBranchRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


    }
}