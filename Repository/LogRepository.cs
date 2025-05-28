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

        internal async Task InsertAllLogsToDB(IEnumerable<Log> logs)
        {
            // Build a lookup for existing logs by (UserId, Date)
            var existingLogs = await _context.Logs
                .Select(l => new { l.Id, l.UserId, l.Date })
                .ToListAsync();

            var logLookup = existingLogs.ToDictionary(
                l => (l.UserId, l.Date), l => l.Id);

            int skipped = 0, inserted = 0, updated = 0;

            foreach (var log in logs)
            {
                if (log.UserId == 0 || log.Date == default)
                {
                    skipped++;
                    continue;
                }

                var key = (log.UserId, log.Date);

                if (logLookup.TryGetValue(key, out int existingLogId))
                {
                    // Update existing log (do not update Id)
                    var existing = await _context.Logs.FindAsync(existingLogId);
                    if (existing != null)
                    {
                        continue;
                    }
                }
                else
                {
                    // New log: reset Id to avoid PK conflict
                    log.Id = 0;
                    await _context.Logs.AddAsync(log);
                    inserted++;
                }
            }
            await _context.SaveChangesAsync();
            // Optionally: log or return stats
        }
    }
}