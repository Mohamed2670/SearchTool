using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class BranchRepository : GenericRepository<Branch>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public BranchRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        internal async Task<MainCompany> GetMainCompanyByBranchId(int branchId)
        {
            var branch = await _context.Branches
                .Include(b => b.MainCompany)
                .Include(b=> b.MainCompany.ClassType)
                .FirstOrDefaultAsync(b => b.Id == branchId);
            return branch?.MainCompany;
        }
    }
}