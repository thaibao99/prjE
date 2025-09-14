using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjetax.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace prjetax.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context) => _context = context;

        private bool IsAdmin => HttpContext.Session.GetInt32("IsAdmin") == 1;
        private int? ManagerId => HttpContext.Session.GetInt32("ManagerId");

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            // ===== Thống kê Doanh nghiệp =====
            var q = _context.Enterprises.AsQueryable();
            if (!IsAdmin)
                q = q.Where(e => e.ManagerId == ManagerId);

            var totalEnterprise = await q.CountAsync();
            var active = await q.CountAsync(e => e.Status == "Hoạt động");
            var inactive = totalEnterprise - active;

            // ===== Thống kê Công việc =====
            var now = DateTime.Today;
            var qWork = _context.WorkItems.AsNoTracking().AsQueryable();
            if (!IsAdmin && ManagerId.HasValue)
                qWork = qWork.Where(w => w.ManagerId == ManagerId.Value);

            var totalWork = await qWork.CountAsync();
            var doing = await qWork.CountAsync(w => w.Status == WorkStatus.Doing);
            var overdue = await qWork.CountAsync(w =>
                                (w.Status == WorkStatus.Todo || w.Status == WorkStatus.Doing)
                                && w.DueDate < now);
            var done = await qWork.CountAsync(w => w.Status == WorkStatus.Done);

            var vm = new DashboardViewModel
            {
                // Doanh nghiệp
                Total = totalEnterprise,
                Active = active,
                Inactive = inactive,
                Enterprises = await q
                    .Include(e => e.Manager)
                    .OrderByDescending(e => e.Id)
                    .Take(10)
                    .ToListAsync(),

                // Công việc
                TotalWork = totalWork,
                Doing = doing,
                Overdue = overdue,
                Done = done
            };

            return View(vm);
        }
    }
}
