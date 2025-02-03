using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Models;
using ServerSide.Models;

namespace SearchTool_ServerSide.Data
{
    public class SearchToolDBContext : DbContext
    {
        public SearchToolDBContext(DbContextOptions<SearchToolDBContext> options) : base(options)
        {

        }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<DrugClass> DrugClasses { get; set; }
        public DbSet<DrugInsurance>DrugInsurances { get; set; }
        public DbSet<Insurance>Insurances { get; set;}
        public DbSet<Script> Scripts { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DrugInsurance>(entity =>
            {
                entity.HasKey(item => new { item.InsuranceId, item.DrugId });
            });
        }
    }
}