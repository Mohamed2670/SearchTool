using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class OrderRepository : GenericRepository<Order>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public OrderRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        internal async Task<IEnumerable<Order>> GetAllOrdersByUserId(string UserEmail)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserEmail == UserEmail)
                .ToListAsync();


            return orders;
            
        }
    }
}