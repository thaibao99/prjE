using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;     // <-- cần cho Include, ToListAsync, FirstOrDefaultAsync
using prjetax.Models;                    // <-- giữ 1 namespace models duy nhất (xóa dòng PrjEtax.Models)
using System.Linq;
using System.Threading.Tasks;
public class WorkLogsController : Controller
{
    private readonly AppDbContext _db;
    public WorkLogsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int workItemId)
    {
        var me = HttpContext.Session.GetInt32("ManagerId") ?? 0;
        var wi = await _db.WorkItems.AsNoTracking()
                                    .FirstOrDefaultAsync(w => w.Id == workItemId);

        if (wi == null || (wi.ManagerId != me && !(HttpContext.Session.GetInt32("IsAdmin") == 1)))
            return StatusCode(403);

        ViewBag.WorkItem = wi;

        var logs = await _db.WorkLogs
                            .Where(l => l.WorkItemId == workItemId)
                            .OrderByDescending(l => l.At)
                            .ToListAsync();

        // === thêm thống kê nhắc việc ===
        var now = DateTime.Today;
        var in2 = now.AddDays(2);
        var qWork = _db.WorkItems.AsNoTracking().AsQueryable();
        if (HttpContext.Session.GetInt32("IsAdmin") != 1)
            qWork = qWork.Where(w => w.ManagerId == me);

        var dueSoon = await qWork
            .Where(w => (w.Status == WorkStatus.Todo || w.Status == WorkStatus.Doing)
                     && w.DueDate != null && w.DueDate.Value.Date == in2)
            .OrderBy(w => w.DueDate).Take(5).ToListAsync();

        var late = await qWork
            .Where(w => (w.Status == WorkStatus.Todo || w.Status == WorkStatus.Doing)
                     && w.DueDate != null && w.DueDate.Value.Date < now)
            .OrderBy(w => w.DueDate).Take(5).ToListAsync();

        ViewBag.DueSoon = dueSoon;
        ViewBag.OverdueList = late;
        // ===============================

        return View(logs);
    }


    public IActionResult Create(int workItemId) => View(new WorkLog { WorkItemId = workItemId });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkLog m, IFormFile? attachment)
    {
        if (attachment != null && attachment.Length > 0)
        {
            using var ms = new MemoryStream();
            await attachment.CopyToAsync(ms);
            m.Attachment = ms.ToArray();
            m.AttachmentName = attachment.FileName;
        }
        _db.Add(m); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { workItemId = m.WorkItemId });
    }

    public async Task<FileResult> Download(int id)
    {
        var l = await _db.WorkLogs.FindAsync(id);
        return File(l!.Attachment!, "application/octet-stream", l.AttachmentName);
    }
}
