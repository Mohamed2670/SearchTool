using AutoMapper;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class FeedBackRepository
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;

        public FeedBackRepository(SearchToolDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

    }
}