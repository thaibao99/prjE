using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjetax.Models;

public class MonitorController : Controller
{
    private readonly AppDbContext _db;
    public MonitorController(AppDbContext db) => _db = db;

    private bool IsAdmin => HttpContext.Session.GetInt32("IsAdmin") == 1;
    private int MeId => HttpContext.Session.GetInt32("ManagerId") ?? 0;

    // /Monitor
    public async Task<IActionResult> Index(string? q, WorkStatus? s, int? managerId, int? enterpriseId,
                                           DateTime? from, DateTime? to)
    {
        var today = DateTime.Today;
        var t2 = today.AddDays(2);

        // Query gốc
        var qWork = _db.WorkItems
            .Include(w => w.Manager)
            .Include(w => w.Enterprise)
            .AsQueryable();

        // Nếu không phải admin thì chỉ xem được việc của chính mình
        if (!IsAdmin) qWork = qWork.Where(w => w.ManagerId == MeId);

        // Áp bộ lọc
        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim();
            qWork = qWork.Where(w =>
                EF.Functions.Like(w.Title, $"%{k}%") ||
                EF.Functions.Like(w.Enterprise.TaxCode, $"%{k}%") ||
                EF.Functions.Like(w.Enterprise.TaxPayerName, $"%{k}%")
            );
        }
        if (s.HasValue)
            qWork = qWork.Where(w => w.Status == s);
        if (managerId.HasValue)
            qWork = qWork.Where(w => w.ManagerId == managerId.Value);
        if (enterpriseId.HasValue)
            qWork = qWork.Where(w => w.EnterpriseId == enterpriseId.Value);
        if (from.HasValue)
            qWork = qWork.Where(w => w.DueDate == null || w.DueDate >= from.Value);
        if (to.HasValue)
            qWork = qWork.Where(w => w.DueDate == null || w.DueDate <= to.Value);

        // Số liệu tổng quan
        var total = await qWork.CountAsync();
        var doing = await qWork.CountAsync(w => w.Status == WorkStatus.Doing);
        var done = await qWork.CountAsync(w => w.Status == WorkStatus.Done);
        var overdue = await qWork.CountAsync(w =>
                        (w.Status == WorkStatus.Todo || w.Status == WorkStatus.Doing) &&
                         w.DueDate != null && w.DueDate.Value.Date < today);
        var dueSoon = await qWork.CountAsync(w =>
                        (w.Status == WorkStatus.Todo || w.Status == WorkStatus.Doing) &&
                         w.DueDate != null && w.DueDate.Value.Date == t2);

        // Lấy danh sách hiển thị
        var items = await qWork
            .OrderBy(w => w.DueDate ?? DateTime.MaxValue)
            .ThenBy(w => w.Status)
            .Take(300) // giới hạn hiển thị
            .ToListAsync();

        // Select list cho bộ lọc
        ViewBag.Managers = new SelectList(
            await _db.Managers.OrderBy(m => m.Name).ToListAsync(), "Id", "Name", managerId);
        ViewBag.Enterprises = new SelectList(
            await _db.Enterprises.OrderBy(e => e.TaxPayerName).ToListAsync(), "Id", "TaxPayerName", enterpriseId);

        var vm = new WorkMonitorVM
        {
            Q = q,
            S = s,
            ManagerId = managerId,
            EnterpriseId = enterpriseId,
            From = from,
            To = to,
            Total = total,
            Doing = doing,
            Done = done,
            Overdue = overdue,
            DueSoon = dueSoon,
            Items = items
        };

        return View(vm);
    }
}
