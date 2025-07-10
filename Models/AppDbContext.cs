using Microsoft.EntityFrameworkCore;

namespace prjetax.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts)
            : base(opts) { }

        public DbSet<Manager> Managers { get; set; }
        public DbSet<EnterpriseDemo> Enterprises { get; set; }
        public DbSet<EnterpriseHistory> EnterpriseHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<EnterpriseDemo>()
              .HasOne(e => e.Manager)
              .WithMany(m => m.Enterprises)
              .HasForeignKey(e => e.ManagerId)
              .OnDelete(DeleteBehavior.SetNull);

            mb.Entity<EnterpriseHistory>()
              .HasOne(h => h.Enterprise)
              .WithMany()
              .HasForeignKey(h => h.EnterpriseId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
