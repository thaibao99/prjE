using Microsoft.EntityFrameworkCore;
using prjetax.Models;
using PrjEtax.Services;

public class ReminderWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ReminderWorker> _logger;
    public ReminderWorker(IServiceProvider sp, ILogger<ReminderWorker> logger)
    { _sp = sp; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var runAt = now.Date.AddHours(8); // 08:00
            var delay = runAt > now ? runAt - now : runAt.AddDays(1) - now;
            await Task.Delay(delay, stoppingToken);

            try { await RunOnce(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "ReminderWorker failed"); }
        }
    }

    private async Task RunOnce(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mail = scope.ServiceProvider.GetRequiredService<EmailService>();

        var today = DateTime.Today;
        var in2Days = today.AddDays(2);

        // 1) Nhắc trước 2 ngày: Todo/Doing có DueDate đúng in2Days
        var coming = await db.WorkItems
            .Include(w => w.Manager)
            .Include(w => w.Enterprise)
            .Where(w => (w.Status == WorkStatus.Todo || w.Status == WorkStatus.Doing)
                     && w.DueDate != null && w.DueDate.Value.Date == in2Days)
            .ToListAsync(ct);

        foreach (var w in coming)
        {
            var to = w.Manager.Email; // nếu có cột Email thì dùng Email
            if (string.IsNullOrWhiteSpace(to)) continue;

            var subject = $"[Nhắc việc] {w.Title} – đến hạn {w.DueDate:dd/MM/yyyy}";
            var body =
$@"Chào {w.Manager.Name},

Công việc: {w.Title}
Doanh nghiệp: {w.Enterprise?.TaxPayerName ?? "(không gắn DN)"}
Hạn xử lý: {w.DueDate:dd/MM/yyyy}

Vui lòng hoàn thành trước hạn.
-- eTax Enterprise";
            await mail.SendEmailAsync(to, subject, body);
        }

        // 2) Quá hạn: DueDate < today và chưa Done
        var overdue = await db.WorkItems
            .Include(w => w.Manager)
            .Include(w => w.Enterprise)
            .Where(w => (w.Status == WorkStatus.Todo || w.Status == WorkStatus.Doing)
                     && w.DueDate != null && w.DueDate.Value.Date < today)
            .ToListAsync(ct);
        foreach (var w in overdue)
        {
            var to = w.Manager.Username;
            if (string.IsNullOrWhiteSpace(to)) continue;
            await mail.SendEmailAsync(
                to,
                $"[QUÁ HẠN] {w.Title} – hạn {w.DueDate:dd/MM/yyyy}",
$@"Công việc '{w.Title}' đã quá hạn {today.Subtract(w.DueDate!.Value.Date).Days} ngày.
Doanh nghiệp: {w.Enterprise?.TaxPayerName ?? "(không gắn DN)"}.

Vui lòng cập nhật trạng thái/hoàn thành sớm.");
        }
    }
}
