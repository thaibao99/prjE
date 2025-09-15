using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjetax.Models;
using prjetax.Services;
using prjetax.Helpers;
using System.Text.Json;

public class EnterprisesImportController : Controller
{
    private readonly AppDbContext _db;
    private readonly EnterpriseExcelService _excel;
    private readonly ILogger<EnterprisesImportController> _log;

    public EnterprisesImportController(AppDbContext db, EnterpriseExcelService excel, ILogger<EnterprisesImportController> log)
    {
        _db = db; _excel = excel; _log = log;
    }

    // GET: tải file mẫu
    public IActionResult Template()
    {
        var bytes = _excel.BuildTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EnterpriseTemplate.xlsx");
    }

    // GET: Upload form (nếu mở trang riêng)
    public IActionResult Upload() => View();

    // POST: Đọc Excel → Preview
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Vui lòng chọn file Excel (.xlsx).");
            return View();
        }

        List<string> errors;
        List<ExcelEnterpriseRow> rows;
        using (var stream = file.OpenReadStream())
        {
            rows = _excel.ReadFromExcel(stream, out errors);
        }

        var vm = new ImportPreviewVM { Errors = errors };

        if (!rows.Any() && !errors.Any())
            vm.Errors.Add("Không có dữ liệu hợp lệ trong file.");

        // So khớp theo TaxCode
        var taxCodes = rows.Select(r => r.TaxCode).Distinct().ToList();
        var existing = await _db.Enterprises
                                .Where(e => taxCodes.Contains(e.TaxCode))
                                .ToListAsync();
        var map = existing.ToDictionary(e => e.TaxCode, e => e);

        foreach (var r in rows)
        {
            if (map.TryGetValue(r.TaxCode, out var ex))
                vm.Conflicts.Add(new ImportConflict { Incoming = r, Existing = ex });
            else
                vm.NewItems.Add(r);
        }

        // Lưu state vào Session
        var token = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString("IMPORT_" + token, JsonSerializer.Serialize(vm));
        vm.TempToken = token;

        return View("Preview", vm);
    }

    // POST: Xác nhận → ghi DB & xuất báo cáo
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(string token, List<bool> overwriteFlags)
    {
        var raw = HttpContext.Session.GetString("IMPORT_" + token);
        if (raw == null) return RedirectToAction(nameof(Upload));

        var vm = JsonSerializer.Deserialize<ImportPreviewVM>(raw)!;

        for (int i = 0; i < vm.Conflicts.Count && i < overwriteFlags.Count; i++)
            vm.Conflicts[i].Overwrite = overwriteFlags[i];

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Thêm mới
            foreach (var n in vm.NewItems)
            {
                _db.Enterprises.Add(n.ToEntity());
            }

            // Ghi đè
            var changed = new List<ImportConflict>();
            foreach (var c in vm.Conflicts.Where(x => x.Overwrite))
            {
                var ex = await _db.Enterprises.FirstAsync(e => e.TaxCode == c.Incoming.TaxCode);

                // snapshot BEFORE để báo cáo
                c.Existing = new EnterpriseDemo
                {
                    Id = ex.Id,
                    TaxAgencyCode = ex.TaxAgencyCode,
                    TaxAgencyName = ex.TaxAgencyName,
                    TaxPayer = ex.TaxPayer,
                    TaxCode = ex.TaxCode,
                    TaxPayerName = ex.TaxPayerName,
                    ManagementDept = ex.ManagementDept,
                    ManagerName = ex.ManagerName,
                    MainBusinessCode = ex.MainBusinessCode,
                    MainBusinessName = ex.MainBusinessName,
                    BusinessDescription = ex.BusinessDescription,
                    DirectorName = ex.DirectorName,
                    DirectorPhone = ex.DirectorPhone,
                    ChiefAccountant = ex.ChiefAccountant,
                    ChiefAccountantPhone = ex.ChiefAccountantPhone,
                    DocumentNumber = ex.DocumentNumber,
                    DocumentDate = ex.DocumentDate,
                    DocumentType = ex.DocumentType,
                    Email = ex.Email,
                    ManagerId = ex.ManagerId,
                    Status = ex.Status,
                    Notes = ex.Notes
                };

                // APPLY AFTER
                var after = c.Incoming;
                ex.TaxAgencyCode = after.TaxAgencyCode;
                ex.TaxAgencyName = after.TaxAgencyName;
                ex.TaxPayer = after.TaxPayer;
                // ex.TaxCode            = after.TaxCode; // KHÔNG đổi khóa
                ex.TaxPayerName = after.TaxPayerName;
                ex.ManagementDept = after.ManagementDept;
                ex.ManagerName = after.ManagerName;
                ex.MainBusinessCode = after.MainBusinessCode;
                ex.MainBusinessName = after.MainBusinessName;
                ex.BusinessDescription = after.BusinessDescription;
                ex.DirectorName = after.DirectorName;
                ex.DirectorPhone = after.DirectorPhone;
                ex.ChiefAccountant = after.ChiefAccountant;
                ex.ChiefAccountantPhone = after.ChiefAccountantPhone;
                ex.DocumentNumber = after.DocumentNumber;
                ex.DocumentDate = after.DocumentDate;
                ex.DocumentType = after.DocumentType;
                ex.Email = after.Email;
                ex.ManagerId = after.ManagerId;
                ex.Status = after.Status;
                ex.Notes = after.Notes;

                changed.Add(c);
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // Xuất Excel báo cáo thay đổi
            var report = _excel.BuildChangeReport(changed);
            return File(report, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"BaoCao_ThayDoi_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _log.LogError(ex, "Import failed");
            TempData["err"] = "Không thể nhập dữ liệu. Vui lòng thử lại.";
            return RedirectToAction(nameof(Upload));
        }
    }
}
