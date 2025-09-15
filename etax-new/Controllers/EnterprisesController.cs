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
            // 1) Phân quyền & bộ lọc cán bộ hiệu lực
            bool isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;

            // Admin: dùng tham số managerId từ query (null = tất cả)
            // Non-admin: luôn khóa theo ManagerId trong session
            int? currentManagerId = isAdmin
                                    ? managerId
                                    : HttpContext.Session.GetInt32("ManagerId");

            // 2) Chuẩn bị dropdown cán bộ cho Admin
            if (isAdmin)
            {
                var allManagers = await _context.Managers
                                                .OrderBy(m => m.Name)
                                                .ToListAsync();

                // SelectList này chỉ chứa danh sách cán bộ thật
                // Dòng "Tất cả cán bộ" sẽ render thủ công ở View (option value="")
                ViewBag.Managers = new SelectList(allManagers, "Id", "Name");
            }

            // 3) Đưa thông tin cho View
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Search = search;                         // giữ từ khóa
            ViewBag.SelectedManagerId = currentManagerId;    // để giữ lựa chọn dropdown

            // 4) Xây dựng truy vấn
            var q = _context.Enterprises
                            .Include(e => e.Manager)
                            .AsQueryable();

            // Tìm theo MÃ SỐ THUẾ (và có thể kèm tên DN nếu muốn)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(e =>
                    EF.Functions.Like(e.TaxCode, $"%{s}%")      // mã số thuế
                    || EF.Functions.Like(e.TaxPayerName, $"%{s}%")  // mở rộng: tên DN
                );
            }

            // Lọc theo cán bộ nếu có (admin chọn cụ thể hoặc non-admin bị khóa)
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
            ViewBag.Statuses = GetStatusSelectList();
            return View();
        }

        // POST: /Enterprises/Create
        // POST: /Enterprises/Create
        private SelectList GetStatusSelectList(string? selected = null)
        {
            // Các giá trị cố định
            var items = new[]
            {
        new { Value = "Hoạt động",      Text = "Hoạt động" },
        new { Value = "Tạm nghỉ",       Text = "Tạm nghỉ" },
        new { Value = "Dừng hoạt động", Text = "Dừng hoạt động" }
    };
            return new SelectList(items, "Value", "Text", selected);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnterpriseDemo model)
        {ViewBag.Statuses = GetStatusSelectList();
            // 1. Nếu validation lỗi → refill dropdown và trả về View
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                                                .SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage));
                // log errors
                Console.WriteLine("ModelState Errors: " + errors);
                var allManagers = await _context.Managers
                                                .OrderBy(m => m.Name)
                                                .ToListAsync();
                ViewBag.Managers = new SelectList(allManagers, "Id", "Name", model.ManagerId);
                // Có thể thêm TempData["Error"] = "…"; để hiển thị dưới summary
                ViewBag.Statuses = GetStatusSelectList(model.Status);
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

            // **Thêm dòng này** để view Create show thông báo
            TempData["Success"] = "Thêm mới doanh nghiệp thành công.";

            // Redirect về Index (hoặc nếu bạn muốn ở lại Create, thì return View())
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
            ViewBag.Statuses = GetStatusSelectList();
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
                ViewBag.Statuses = GetStatusSelectList(model.Status);
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
