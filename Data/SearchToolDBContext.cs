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
        public DbSet<DrugInsurance> DrugInsurances { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<Script> Scripts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ClassInsurance> ClassInsurances { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DrugInsurance>(entity =>
            {
                entity.HasKey(item => new { item.InsuranceId, item.DrugId });
            });
            modelBuilder.Entity<ClassInsurance>(entity =>
            {
                entity.HasKey(ci => new { ci.InsuranceId, ci.ClassId, ci.Date });

                // Define foreign key relationships
                entity.HasOne(ci => ci.Insurance)
                    .WithMany()
                    .HasForeignKey(ci => ci.InsuranceId)
                    .OnDelete(DeleteBehavior.Cascade); // Adjust delete behavior if necessary

                entity.HasOne(ci => ci.DrugClass)
                    .WithMany()
                    .HasForeignKey(ci => ci.ClassId)
                    .OnDelete(DeleteBehavior.Cascade); // Adjust delete behavior if necessary
            });
        }
    }
}