using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class UserRepository : GenericRepository<User>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public UserRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        internal async Task AddAllUserData(IEnumerable<User> items)
        {
            var localUsers = await _context.Users
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            var emailToUserId = localUsers.ToDictionary(u => u.Email.ToLower(), u => u.Id);

            int skipped = 0, inserted = 0, updated = 0;

            foreach (var user in items)
            {
                if (string.IsNullOrWhiteSpace(user.Email) || user.Password =="DefaultPass123")
                {
                    skipped++;
                    continue;
                }

                var emailKey = user.Email.ToLower();

                if (emailToUserId.TryGetValue(emailKey, out int existingUserId))
                {
                    // Update existing user (do not update Id)
                    var existing = await _context.Users.FindAsync(existingUserId);
                    if (existing != null)
                    {
                        existing.Email = user.Email;
                        existing.ShortName = user.ShortName;
                        existing.Name = user.Name;
                        existing.Password = user.Password;
                        existing.BranchId = user.BranchId;
                        existing.Role = user.Role;
                        updated++;
                    }
                }
                else
                {
                    // New user: reset Id to avoid PK conflict
                    user.Id = 0;
                    await _context.Users.AddAsync(user);
                    inserted++;
                }
            }
            await _context.SaveChangesAsync();
            // Optionally: return stats or log them
        }

        internal async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
        }

    }
}