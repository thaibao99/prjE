using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using prjetax.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace prjetax.Controllers
{
    public class EnterprisesController : Controller
    {
        private readonly AppDbContext _context;
        public EnterprisesController(AppDbContext context) => _context = context;

        private bool IsAdmin => HttpContext.Session.GetInt32("IsAdmin") == 1;
        private int? ManagerId => HttpContext.Session.GetInt32("ManagerId");

        // GET: /Enterprises
        public async Task<IActionResult> Index(string search, int? managerId)
        {
            // Lấy danh sách cán bộ để đổ vào dropdown
            var allManagers = await _context.Managers
                                            .OrderBy(m => m.Name)
                                            .ToListAsync();

            // Tạo SelectList: Id là value, FullName là text, managerId là selectedValue
            ViewBag.Managers = new SelectList(allManagers, "Id", "Name", managerId);
            ViewBag.Search = search;

            // Query chính: lọc theo từ khóa và theo managerId nếu có
            var q = _context.Enterprises
                            .Include(e => e.Manager)
                            .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(e => e.TaxPayerName.Contains(search));
            if (managerId.HasValue)
                q = q.Where(e => e.ManagerId == managerId.Value);

            var list = await q.ToListAsync();
            return View(list);
        }


        // GET: /Enterprises/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Managers = await _context.Managers.ToListAsync();
            return View();
        }

        // POST: /Enterprises/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnterpriseDemo model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Managers = await _context.Managers.ToListAsync();
                return View(model);
            }
            _context.Enterprises.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Enterprises/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ent = await _context.Enterprises.FindAsync(id);
            if (ent == null) return NotFound();
            ViewBag.Managers = await _context.Managers.ToListAsync();
            return View(ent);
        }

        // POST: /Enterprises/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnterpriseDemo model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Managers = await _context.Managers.ToListAsync();
                return View(model);
            }

            var ent = await _context.Enterprises.FindAsync(id);
            if (ent == null) return NotFound();

            // Cập nhật các trường
            ent.TaxAgencyCode = model.TaxAgencyCode;
            ent.TaxAgencyName = model.TaxAgencyName;
            ent.TaxCode = model.TaxCode;
            ent.TaxPayerName = model.TaxPayerName;
            ent.MainBusinessName = model.MainBusinessName;
            ent.BusinessDescription = model.BusinessDescription;
            ent.DirectorName = model.DirectorName;
            ent.DirectorPhone = model.DirectorPhone;
            ent.ChiefAccountant = model.ChiefAccountant;
            ent.ChiefAccountantPhone = model.ChiefAccountantPhone;
            ent.Email = model.Email;
            ent.ManagerId = model.ManagerId;
            ent.Status = model.Status;
            ent.Notes = model.Notes;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Enterprises/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ent = await _context.Enterprises
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (ent == null) return NotFound();
            return View(ent);
        }

        // GET: /Enterprises/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var ent = await _context.Enterprises.FindAsync(id);
            if (ent == null) return NotFound();
            return View(ent);
        }

        // POST: /Enterprises/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ent = await _context.Enterprises.FindAsync(id);
            _context.Enterprises.Remove(ent);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
