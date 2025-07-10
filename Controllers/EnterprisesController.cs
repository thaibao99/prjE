using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using prjetax.Models;
using System.Linq;
using System.Threading.Tasks;

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
            // Đổ dropdown né
            ViewBag.Managers = await _context.Managers.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Search = search;
            ViewBag.SelectedManagerId = managerId;

            var q = _context.Enterprises.Include(e => e.Manager).AsQueryable();

            if (!IsAdmin)
            {
                // Manager chỉ thấy DN mình quản lý
                q = q.Where(e => e.ManagerId == ManagerId);
            }
            else if (managerId.HasValue)
            {
                q = q.Where(e => e.ManagerId == managerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(e => e.TaxPayerName.Contains(search));

            return View(await q.ToListAsync());
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
