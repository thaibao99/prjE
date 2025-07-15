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
            // 1) Determine role & effective manager filter
            bool isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;
            // Admin can pass in any managerId; non-admins are fixed to their own ID
            int? currentManagerId = isAdmin
                                    ? managerId
                                    : HttpContext.Session.GetInt32("ManagerId");

            // 2) If admin, prepare the dropdown of all managers
            // EnterprisesController.cs, trong Index action
            if (isAdmin)
            {
                var allManagers = await _context.Managers
                                                .OrderBy(m => m.Name)
                                                .ToListAsync();
                // value = Id, text = Name (khớp với property), selected = managerId
                ViewBag.Managers = new SelectList(allManagers, "Id", "Name", managerId);
            }


            // 3) Store for use in the View
            ViewBag.Search = search;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.SelectedManagerId = currentManagerId;

            // 4) Build your enterprise query
            var q = _context.Enterprises
                            .Include(e => e.Manager)
                            .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(e => e.TaxPayerName.Contains(search));

            if (currentManagerId.HasValue)
                q = q.Where(e => e.ManagerId == currentManagerId.Value);

            var list = await q.ToListAsync();
            return View(list);
        }



        // GET: /Enterprises/Create
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách managers để dropdown
            var allManagers = await _context.Managers
                                            .OrderBy(m => m.Name)
                                            .ToListAsync();
            ViewBag.Managers = new SelectList(allManagers, "Id", "Name");

            return View();
        }

        // POST: /Enterprises/Create
        // POST: /Enterprises/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnterpriseDemo model)
        {
            // 1. Nếu validation lỗi → refill dropdown và trả về View
            if (!ModelState.IsValid)
            {
                var allManagers = await _context.Managers
                                                .OrderBy(m => m.Name)
                                                .ToListAsync();
                ViewBag.Managers = new SelectList(allManagers, "Id", "Name", model.ManagerId);
                return View(model);
            }

            // 2. Map thủ công tất cả các field từ view-model sang entity
            var ent = new EnterpriseDemo
            {
                // CHỈ ĐỂ Id = 0 để EF tự generate
                // (nếu bảng có Identity)
                TaxAgencyCode = model.TaxAgencyCode,
                TaxAgencyName = model.TaxAgencyName,
                TaxPayer = model.TaxPayer,
                TaxCode = model.TaxCode,
                TaxPayerName = model.TaxPayerName,
                ManagementDept = model.ManagementDept,
                ManagerName = model.ManagerName,
                MainBusinessCode = model.MainBusinessCode,
                MainBusinessName = model.MainBusinessName,
                BusinessDescription = model.BusinessDescription,
                DirectorName = model.DirectorName,
                DirectorPhone = model.DirectorPhone,
                ChiefAccountant = model.ChiefAccountant,
                ChiefAccountantPhone = model.ChiefAccountantPhone,
                DocumentNumber = model.DocumentNumber,
                DocumentDate = model.DocumentDate,
                DocumentType = model.DocumentType,
                Email = model.Email,
                ManagerId = model.ManagerId,
                Status = model.Status,
                Notes = model.Notes
            };

            // 3. Thêm và lưu
            _context.Enterprises.Add(ent);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: /Enterprises/Edit/5
        // GET: /Enterprises/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ent = await _context.Enterprises.FindAsync(id);
            if (ent == null) return NotFound();

            // Lấy danh sách managers, đánh dấu ent.ManagerId là selected
            var allManagers = await _context.Managers
                                            .OrderBy(m => m.Name)
                                            .ToListAsync();
            ViewBag.Managers = new SelectList(allManagers, "Id", "Name", ent.ManagerId);

            return View(ent);
        }

        // POST: /Enterprises/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnterpriseDemo model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                // Nếu validation lỗi, refill dropdown với selected hiện tại
                var allManagers = await _context.Managers
                                                .OrderBy(m => m.Name)
                                                .ToListAsync();
                ViewBag.Managers = new SelectList(allManagers, "Id", "Name", model.ManagerId);
                return View(model);
            }

            var ent = await _context.Enterprises.FindAsync(id);
            if (ent == null) return NotFound();

            // Map các trường từ model lên entity
            ent.TaxAgencyCode = model.TaxAgencyCode;
            ent.TaxAgencyName = model.TaxAgencyName;
            ent.TaxPayer = model.TaxPayer;
            ent.TaxCode = model.TaxCode;
            ent.TaxPayerName = model.TaxPayerName;
            ent.ManagementDept = model.ManagementDept;
            ent.ManagerName = model.ManagerName;
            ent.MainBusinessCode = model.MainBusinessCode;
            ent.MainBusinessName = model.MainBusinessName;
            ent.BusinessDescription = model.BusinessDescription;
            ent.DirectorName = model.DirectorName;
            ent.DirectorPhone = model.DirectorPhone;
            ent.ChiefAccountant = model.ChiefAccountant;
            ent.ChiefAccountantPhone = model.ChiefAccountantPhone;
            ent.DocumentNumber = model.DocumentNumber;
            ent.DocumentDate = model.DocumentDate;
            ent.DocumentType = model.DocumentType;
            ent.Email = model.Email;
            ent.ManagerId = model.ManagerId;
            ent.Status = model.Status;
            ent.Notes = model.Notes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật doanh nghiệp thành công.";
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
