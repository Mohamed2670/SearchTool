using Microsoft.EntityFrameworkCore;

namespace SearchTool_ServerSide.Data
{
    public class SearchToolDBContext : DbContext
    {
        public SearchToolDBContext(DbContextOptions<SearchToolDBContext>options) : base(options)
        {
            
        }
    }
}