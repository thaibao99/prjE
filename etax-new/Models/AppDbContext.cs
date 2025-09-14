using Microsoft.EntityFrameworkCore;
using prjetax.Models;

namespace prjetax.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts)
            : base(opts)
        {
        }

        // 1. Chỉ giữ mỗi DbSet<EnterpriseDemo> một lần, 
        //    và đặt tên sao cho khớp trong controller/view (_context.Enterprises)
        public DbSet<EnterpriseDemo> Enterprises { get; set; }
        public DbSet<WorkItem> WorkItems => Set<WorkItem>();
        public DbSet<WorkLog> WorkLogs => Set<WorkLog>();

        public DbSet<Manager> Managers { get; set; }
        public DbSet<EnterpriseHistory> EnterpriseHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // Nếu trong database bảng tên là "EnterprisesDemo", bạn cần map explicit:
            mb.Entity<EnterpriseDemo>()
              .ToTable("EnterprisesDemo")      // map cho đúng table
              .HasOne(e => e.Manager)
              .WithMany(m => m.Enterprises)    // property ICollection<EnterpriseDemo> Enterprises trong Manager
              .HasForeignKey(e => e.ManagerId)
              .OnDelete(DeleteBehavior.SetNull);

            mb.Entity<EnterpriseHistory>()
              .ToTable("EnterpriseHistories")  // (nếu cần, nếu tên trùng)
              .HasOne(h => h.Enterprise)
              .WithMany()                       // hoặc .WithMany(e => e.Histories) nếu bạn add List<EnterpriseHistory> vào EnterpriseDemo
              .HasForeignKey(h => h.EnterpriseId)
              .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<WorkItem>()
     .HasOne(w => w.Manager).WithMany().HasForeignKey(w => w.ManagerId)
     .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<WorkItem>()
             .HasOne(w => w.Enterprise).WithMany().HasForeignKey(w => w.EnterpriseId)
             .OnDelete(DeleteBehavior.SetNull);

           mb.Entity<WorkLog>()
             .HasOne(l => l.WorkItem).WithMany(w => w.Logs).HasForeignKey(l => l.WorkItemId);
        }
    }
}
