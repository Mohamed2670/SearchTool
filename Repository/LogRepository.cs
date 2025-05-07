using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Dtos.LogDtos;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class LogRepository : GenericRepository<Log>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public LogRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ICollection<LogsReadDto>> GetAll(int userId)
        {
            // Retrieve the user including their branch info to get the main company id
            var user = await _context.Users
                                     .Include(u => u.Branch)
                                     .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Get the main company id from the user's branch
            var mainCompanyId = user.Branch.MainCompanyId;

            // Query logs for all users whose branch belongs to the same main company
            var items = await (
                from log in _context.Logs
                join usr in _context.Users on log.UserId equals usr.Id
                join branch in _context.Branches on usr.BranchId equals branch.Id
                where branch.MainCompanyId == mainCompanyId
                select new LogsReadDto
                {
                    Id = log.UserId,
                    UserName = usr.Name,
                    Date = log.Date,
                    Action = log.Action
                }).ToListAsync();

            return items;
        }

    }
}