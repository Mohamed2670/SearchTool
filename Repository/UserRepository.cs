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
        internal async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
        }

    }
}