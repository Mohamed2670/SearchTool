using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class SearchLogRepository : GenericRepository<SearchLog>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public SearchLogRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        internal async Task<SearchLog> GetByOrderItemId(int id)
        {
            var searchLog = await _context.SearchLogs
                .FirstOrDefaultAsync(s => s.OrderItemId == id);

            return searchLog;
        }
    }
}