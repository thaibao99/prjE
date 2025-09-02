using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjetax.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PrjEtax.Controllers
{
    public class EnterpriseHistoriesController : Controller
    {
        private readonly AppDbContext _context;
        public EnterpriseHistoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /EnterpriseHistories/Summary
        public async Task<IActionResult> Summary()
        {
            // 1. Lấy thông tin role & managerId từ session
            bool isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;
            int? managerId = HttpContext.Session.GetInt32("ManagerId");

            // 2. Bắt đầu query Enterprises kèm Manager
            var query = _context.Enterprises
                                .Include(e => e.Manager)
                                .AsQueryable();

            // 3. Nếu không phải Admin, chỉ lọc doanh nghiệp do chính user quản lý
            if (!isAdmin && managerId.HasValue)
            {
                query = query.Where(e => e.ManagerId == managerId.Value);
            }

            // 4. Projection ra các trường cần thiết
            var list = await query
                .Select(e => new {
                    EnterpriseId = e.Id,
                    TaxPayerName = e.TaxPayerName,
                    ManagerName = e.Manager.Name,
                    LatestReminder = _context.EnterpriseHistories
                                        .Where(h => h.EnterpriseId == e.Id)
                                        .OrderByDescending(h => h.Date)
                                        .Select(h => h.Date)
                                        .FirstOrDefault(),
                    MailCount = _context.EnterpriseHistories
                                        .Count(h => h.EnterpriseId == e.Id)
                })
                .ToListAsync();

            // 5. Trả về list trực tiếp làm model cho view
            return View(list);
        }

        // GET: EnterpriseHistories
        // Nếu có ?enterpriseId= thì lọc, ngược lại show toàn bộ

        public async Task<IActionResult> Index(int? enterpriseId)
        {
            // build dropdown list of enterprises
            var list = await _context.Enterprises
                                     .Select(e => new { e.Id, Display = e.TaxPayerName + " (" + e.TaxCode + ")" })
                                     .ToListAsync();
            ViewBag.EnterprisesFilter = new SelectList(list, "Id", "Display", enterpriseId);
            ViewBag.SelectedEnterpriseId = enterpriseId;

            // query histories
            var q = _context.EnterpriseHistories
                            .Include(h => h.Enterprise)
                            .AsQueryable();
            if (enterpriseId.HasValue)
                q = q.Where(h => h.EnterpriseId == enterpriseId.Value);

            var model = await q.ToListAsync();
            return View(model);
        }

        // GET: EnterpriseHistories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var history = await _context.EnterpriseHistories
                                        .Include(h => h.Enterprise)
                                        .FirstOrDefaultAsync(h => h.Id == id);
            if (history == null) return NotFound();
            return View(history);
        }

        // GET: EnterpriseHistories/Create?enterpriseId=...
        public async Task<IActionResult> Create(int? enterpriseId)
        {
            var list = await _context.Enterprises
                                     .Select(e => new { e.Id, Display = e.TaxPayerName + " (" + e.TaxCode + ")" })
                                     .ToListAsync();
            ViewBag.EnterprisesFilter = new SelectList(list, "Id", "Display", enterpriseId);

            var hist = new EnterpriseHistory { EnterpriseId = enterpriseId ?? 0, Date = DateTime.Today };
            return View(hist);
        }

        // POST: EnterpriseHistories/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnterpriseHistory history)
        {
            if (!ModelState.IsValid)
            {
                // rebuild dropdown
                var list = await _context.Enterprises
                                         .Select(e => new { e.Id, Display = e.TaxPayerName + " (" + e.TaxCode + ")" })
                                         .ToListAsync();
                ViewBag.EnterprisesFilter = new SelectList(list, "Id", "Display", history.EnterpriseId);
                return View(history);
            }

            _context.Add(history);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { enterpriseId = history.EnterpriseId });
        }

        // GET: EnterpriseHistories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var history = await _context.EnterpriseHistories.FindAsync(id);
            if (history == null) return NotFound();

            var list = await _context.Enterprises
                                     .Select(e => new { e.Id, Display = e.TaxPayerName + " (" + e.TaxCode + ")" })
                                     .ToListAsync();
            ViewBag.EnterprisesFilter = new SelectList(list, "Id", "Display", history.EnterpriseId);
            return View(history);
        }

        // POST: EnterpriseHistories/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnterpriseHistory history)
        {
            if (id != history.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                var list = await _context.Enterprises
                                         .Select(e => new { e.Id, Display = e.TaxPayerName + " (" + e.TaxCode + ")" })
                                         .ToListAsync();
                ViewBag.EnterprisesFilter = new SelectList(list, "Id", "Display", history.EnterpriseId);
                return View(history);
            }

            _context.Update(history);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { enterpriseId = history.EnterpriseId });
        }

        // GET: EnterpriseHistories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var history = await _context.EnterpriseHistories
                                        .Include(h => h.Enterprise)
                                        .FirstOrDefaultAsync(h => h.Id == id);
            if (history == null) return NotFound();
            return View(history);
        }

        // POST: EnterpriseHistories/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var history = await _context.EnterpriseHistories.FindAsync(id);
            var entId = history.EnterpriseId;
            _context.EnterpriseHistories.Remove(history);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { enterpriseId = entId });
        }
        public async Task<IActionResult> Download(int id)
        {
            var hist = await _context.EnterpriseHistories.FindAsync(id);
            if (hist == null || hist.Document == null)
                return NotFound();

            return File(
                hist.Document,                // byte[] chứa nội dung PDF
                "application/pdf",            // MIME type cho PDF
                hist.DocumentName             // tên file đính kèm (.pdf)
            );
        }
    }
}
