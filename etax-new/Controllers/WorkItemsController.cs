using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;     // <-- cần cho Include, ToListAsync, FirstOrDefaultAsync
using prjetax.Models;                    // <-- giữ 1 namespace models duy nhất (xóa dòng PrjEtax.Models)
using System.Linq;
using System.Threading.Tasks;


public class WorkItemsController : Controller
{
    private readonly AppDbContext _db;
    public WorkItemsController(AppDbContext db) => _db = db;
    private bool IsAdmin => HttpContext.Session.GetInt32("IsAdmin") == 1;
    private int MeId => HttpContext.Session.GetInt32("ManagerId") ?? 0;
    // /WorkItems/My
    public async Task<IActionResult> My(string? q, WorkStatus? s)
    {
        var me = MeId;

        var query = _db.WorkItems
            .Include(w => w.Enterprise)
            .Include(w => w.Manager)
            .AsNoTracking()                           // đọc-only nhanh hơn
            .AsQueryable();

        if (!IsAdmin) query = query.Where(w => w.ManagerId == me);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim();
            query = query.Where(w =>
                EF.Functions.Like(w.Title, $"%{k}%") ||
                EF.Functions.Like(w.Enterprise.TaxCode, $"%{k}%") ||
                EF.Functions.Like(w.Enterprise.TaxPayerName, $"%{k}%")
            );
        }

        if (s.HasValue) query = query.Where(w => w.Status == s);

        var items = await query
            .OrderBy(w => w.DueDate ?? DateTime.MaxValue)
            .ToListAsync();

        return View(items);
    }


    public IActionResult Create()
    {
        ViewBag.Enterprises = new SelectList(_db.Enterprises.OrderBy(e => e.TaxPayerName), "Id", "TaxPayerName");
        if (IsAdmin)
            ViewBag.Managers = new SelectList(_db.Managers.OrderBy(m => m.Name), "Id", "Name");
        return View(new WorkItem { DueDate = DateTime.Today.AddDays(2) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkItem m)
    {
        if (!IsAdmin) m.ManagerId = MeId;         // nhân viên: tự gán cho mình
        if (m.Status == WorkStatus.Done) m.CompletedAt = DateTime.UtcNow;

        ModelState.Remove(nameof(WorkItem.Manager));   // tránh validate navigation
        ModelState.Remove(nameof(WorkItem.Enterprise));

        if (!ModelState.IsValid)
        {
            ViewBag.Enterprises = new SelectList(_db.Enterprises, "Id", "TaxPayerName", m.EnterpriseId);
            if (IsAdmin) ViewBag.Managers = new SelectList(_db.Managers, "Id", "Name", m.ManagerId);
            return View(m);
        }

        _db.WorkItems.Add(m);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(My));
    }


    public async Task<IActionResult> Edit(int id)
    {
        var m = await _db.WorkItems.FindAsync(id);
        if (m == null) return NotFound();

        // chỉ chặn khi KHÔNG phải admin và không phải chủ sở hữu
        if (!IsAdmin && m.ManagerId != MeId) return StatusCode(403);

        ViewBag.Enterprises = new SelectList(_db.Enterprises, "Id", "TaxPayerName", m.EnterpriseId);

        // (tuỳ chọn) cho admin đổi người phụ trách
        if (IsAdmin)
            ViewBag.Managers = new SelectList(_db.Managers.OrderBy(x => x.Name), "Id", "Name", m.ManagerId);

        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(WorkItem m)
    {
        var dbItem = await _db.WorkItems.FindAsync(m.Id);
        if (dbItem == null) return NotFound();

        // chặn nhân viên sửa việc của người khác; admin thì pass
        if (!IsAdmin && dbItem.ManagerId != MeId) return StatusCode(403);

        dbItem.Title = m.Title;
        dbItem.Description = m.Description;
        dbItem.Status = m.Status;
        dbItem.DueDate = m.DueDate;
        dbItem.EnterpriseId = m.EnterpriseId;

        // (tuỳ chọn) admin được đổi người phụ trách
        if (IsAdmin) dbItem.ManagerId = m.ManagerId;

        dbItem.CompletedAt = m.Status == WorkStatus.Done ? DateTime.UtcNow : null;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(My));
    }


    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await _db.WorkItems.FirstOrDefaultAsync(x => x.Id == id);
        if (m == null) return NotFound();

        // admin được xoá; nhân viên chỉ xoá việc của mình
        if (!IsAdmin && m.ManagerId != MeId) return StatusCode(403);

        _db.Remove(m);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(My));
    }

}
