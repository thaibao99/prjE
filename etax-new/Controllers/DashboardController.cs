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
            var q = _context.Enterprises.AsQueryable();
            if (!IsAdmin)
                q = q.Where(e => e.ManagerId == ManagerId);

            var total = await q.CountAsync();
            var active = await q.CountAsync(e => e.Status == "Hoạt động");
            var inactive = total - active;

            var vm = new DashboardViewModel
            {
                Total = total,
                Active = active,
                Inactive = inactive,
                Enterprises = await q
                    .Include(e => e.Manager)
                    .OrderByDescending(e => e.Id)
                    .Take(10)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
