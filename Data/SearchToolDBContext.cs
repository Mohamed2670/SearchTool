using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Models;
using ServerSide.Models;

namespace SearchTool_ServerSide.Data
{
    public class SearchToolDBContext : DbContext
    {
        public SearchToolDBContext(DbContextOptions<SearchToolDBContext> options) : base(options) { }

        public DbSet<Drug> Drugs { get; set; }
        public DbSet<DrugClass> DrugClasses { get; set; }
        public DbSet<DrugInsurance> DrugInsurances { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<Script> Scripts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ClassInsurance> ClassInsurances { get; set; }
        public DbSet<ScriptItem> ScriptItems { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<DrugBranch> DrugBranches { get; set; }
        public DbSet<MainCompany> MainCompanies { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<InsuranceRx> InsuranceRxes { get; set; }
        public DbSet<InsurancePCN> InsurancePCNs { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<SearchLog>SearchLogs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define Composite Keys
            modelBuilder.Entity<DrugInsurance>(entity =>
            {
                entity.HasKey(item => new { item.InsuranceId, item.DrugId, item.BranchId });
            });

            modelBuilder.Entity<ClassInsurance>(entity =>
            {
                entity.HasKey(ci => new { ci.InsuranceId, ci.ClassId, ci.Date, ci.BranchId });

                // Define foreign key relationships
                entity.HasOne(ci => ci.Insurance)
                    .WithMany()
                    .HasForeignKey(ci => ci.InsuranceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.DrugClass)
                    .WithMany()
                    .HasForeignKey(ci => ci.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ci => ci.Branch)
                .WithMany()
                .HasForeignKey(ci => ci.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<DrugBranch>(entity =>
            {
                entity.HasKey(ci => new { ci.DrugId, ci.BranchId });
            });
            modelBuilder.Entity<Specialty>().HasData(
                          new { Id = 1, Name = "Dermatology specialty", }
                      );
            // Seed Data for Branches
            modelBuilder.Entity<MainCompany>().HasData(
               new { Id = 1, Name = "California Dermatology", SpecialtyId = 1 }

           );
            modelBuilder.Entity<Branch>().HasData(
                new Branch { Id = 1, Name = "California Dermatology Institute Thousand Oaks", Location = "Thousand Oaks", Code = "1", MainCompanyId = 1 },
                new Branch { Id = 2, Name = "California Dermatology Institute Northridge", Location = "Northridge", Code = "2", MainCompanyId = 1 },
                new Branch { Id = 3, Name = "California Dermatology Institute Huntington Park", Location = "Huntington Park", Code = "3", MainCompanyId = 1 },
                new Branch { Id = 4, Name = "California Dermatology Institute Palmdale", Location = "Palmdale", Code = "4", MainCompanyId = 1 },
                new Branch { Id = 5, Name = "VIRTUAL", Location = "VIRTUAL", Code = "5", MainCompanyId = 1 }

            );
        }
    }
}
