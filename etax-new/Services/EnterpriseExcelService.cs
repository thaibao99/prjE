using ClosedXML.Excel;
using prjetax.Models;

namespace prjetax.Services
{
    public class EnterpriseExcelService
    {
        // Đọc Excel → danh sách ExcelEnterpriseRow (+ errors)
        public List<ExcelEnterpriseRow> ReadFromExcel(Stream fileStream, out List<string> errors)
        {
            errors = new();
            var rows = new List<ExcelEnterpriseRow>();

            using var wb = new XLWorkbook(fileStream);
            var ws = wb.Worksheets.First(); // sheet đầu

            // Header dòng 1 theo đúng thứ tự cột bên dưới:
            // TaxAgencyCode | TaxAgencyName | TaxPayer | TaxCode | TaxPayerName | ManagementDept
            // ManagerName | MainBusinessCode | MainBusinessName | BusinessDescription
            // DirectorName | DirectorPhone | ChiefAccountant | ChiefAccountantPhone
            // DocumentNumber | DocumentDate | DocumentType | Email | ManagerId | Status | Notes

            int firstDataRow = 2;
            int last = ws.LastRowUsed().RowNumber();

            for (int r = firstDataRow; r <= last; r++)
            {
                var row = new ExcelEnterpriseRow
                {
                    TaxAgencyCode = ws.Cell(r, 1).GetString().Trim(),
                    TaxAgencyName = ws.Cell(r, 2).GetString().Trim(),
                    TaxPayer = ws.Cell(r, 3).GetString().Trim(),
                    TaxCode = ws.Cell(r, 4).GetString().Trim(),
                    TaxPayerName = ws.Cell(r, 5).GetString().Trim(),
                    ManagementDept = ws.Cell(r, 6).GetString().Trim(),
                    ManagerName = ws.Cell(r, 7).GetString().Trim(),
                    MainBusinessCode = ws.Cell(r, 8).GetString().Trim(),
                    MainBusinessName = ws.Cell(r, 9).GetString().Trim(),
                    BusinessDescription = ws.Cell(r, 10).GetString().Trim(),
                    DirectorName = ws.Cell(r, 11).GetString().Trim(),
                    DirectorPhone = ws.Cell(r, 12).GetString().Trim(),
                    ChiefAccountant = ws.Cell(r, 13).GetString().Trim(),
                    ChiefAccountantPhone = ws.Cell(r, 14).GetString().Trim(),
                    DocumentNumber = ws.Cell(r, 15).GetString().Trim(),
                    DocumentDate = ws.Cell(r, 16).TryGetValue<DateTime?>(out var d) ? d : null,
                    DocumentType = ws.Cell(r, 17).GetString().Trim(),
                    Email = ws.Cell(r, 18).GetString().Trim(),
                    ManagerId = int.TryParse(ws.Cell(r, 19).GetString().Trim(), out var mid) ? mid : (int?)null,
                    Status = ws.Cell(r, 20).GetString().Trim(),
                    Notes = ws.Cell(r, 21).GetString().Trim()
                };

                if (string.IsNullOrWhiteSpace(row.TaxCode))
                {
                    errors.Add($"Dòng {r}: thiếu TaxCode (Mã số thuế).");
                    continue;
                }

                rows.Add(row);
            }

            return rows;
        }

        // Xuất báo cáo thay đổi: mỗi DN trùng gồm 2 dòng Trước / Sau liền nhau, diff được tô đậm
        public byte[] BuildChangeReport(List<ImportConflict> changes)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Changes");

            string[] headers = new[]
            {
                "TaxCode","Loại dòng",
                "TaxAgencyCode","TaxAgencyName","TaxPayer","TaxPayerName","ManagementDept","ManagerName",
                "MainBusinessCode","MainBusinessName","BusinessDescription",
                "DirectorName","DirectorPhone","ChiefAccountant","ChiefAccountantPhone",
                "DocumentNumber","DocumentDate","DocumentType","Email","ManagerId","Status","Notes"
            };
            for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

            int r = 2;
            foreach (var c in changes)
            {
                // BEFORE (DB)
                FillRow(ws, r, "Trước", c.Existing);
                ws.Row(r).Style.Fill.BackgroundColor = XLColor.FromHtml("#fff8e1");
                int rowBefore = r;
                r++;

                // AFTER (Excel)
                FillRow(ws, r, "Sau", c.Incoming);
                ws.Row(r).Style.Fill.BackgroundColor = XLColor.FromHtml("#e7f5ff");
                int rowAfter = r;

                // highlight khác biệt cột 3..22 (bỏ TaxCode & Loại dòng)
                for (int col = 3; col <= 22; col++)
                {
                    var before = ws.Cell(rowBefore, col).GetString();
                    var after = ws.Cell(rowAfter, col).GetString();
                    if (!string.Equals(before, after, StringComparison.OrdinalIgnoreCase))
                    {
                        ws.Cell(rowAfter, col).Style.Font.Bold = true;
                        ws.Cell(rowAfter, col).Style.Font.FontColor = XLColor.DarkBlue;
                    }
                }
                r++;
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private void FillRow(IXLWorksheet ws, int r, string lineType, EnterpriseDemo ex)
        {
            ws.Cell(r, 1).Value = ex.TaxCode;
            ws.Cell(r, 2).Value = lineType;
            ws.Cell(r, 3).Value = ex.TaxAgencyCode;
            ws.Cell(r, 4).Value = ex.TaxAgencyName;
            ws.Cell(r, 5).Value = ex.TaxPayer;
            ws.Cell(r, 6).Value = ex.TaxPayerName;
            ws.Cell(r, 7).Value = ex.ManagementDept;
            ws.Cell(r, 8).Value = ex.ManagerName;
            ws.Cell(r, 9).Value = ex.MainBusinessCode;
            ws.Cell(r, 10).Value = ex.MainBusinessName;
            ws.Cell(r, 11).Value = ex.BusinessDescription;
            ws.Cell(r, 12).Value = ex.DirectorName;
            ws.Cell(r, 13).Value = ex.DirectorPhone;
            ws.Cell(r, 14).Value = ex.ChiefAccountant;
            ws.Cell(r, 15).Value = ex.ChiefAccountantPhone;
            ws.Cell(r, 16).Value = ex.DocumentNumber;
            ws.Cell(r, 17).Value = ex.DocumentDate?.ToString("yyyy-MM-dd");
            ws.Cell(r, 18).Value = ex.DocumentType;
            ws.Cell(r, 19).Value = ex.Email;
            ws.Cell(r, 20).Value = ex.ManagerId?.ToString();
            ws.Cell(r, 21).Value = ex.Status;
            ws.Cell(r, 22).Value = ex.Notes;
        }

        private void FillRow(IXLWorksheet ws, int r, string lineType, ExcelEnterpriseRow inc)
        {
            ws.Cell(r, 1).Value = inc.TaxCode;
            ws.Cell(r, 2).Value = lineType;
            ws.Cell(r, 3).Value = inc.TaxAgencyCode;
            ws.Cell(r, 4).Value = inc.TaxAgencyName;
            ws.Cell(r, 5).Value = inc.TaxPayer;
            ws.Cell(r, 6).Value = inc.TaxPayerName;
            ws.Cell(r, 7).Value = inc.ManagementDept;
            ws.Cell(r, 8).Value = inc.ManagerName;
            ws.Cell(r, 9).Value = inc.MainBusinessCode;
            ws.Cell(r, 10).Value = inc.MainBusinessName;
            ws.Cell(r, 11).Value = inc.BusinessDescription;
            ws.Cell(r, 12).Value = inc.DirectorName;
            ws.Cell(r, 13).Value = inc.DirectorPhone;
            ws.Cell(r, 14).Value = inc.ChiefAccountant;
            ws.Cell(r, 15).Value = inc.ChiefAccountantPhone;
            ws.Cell(r, 16).Value = inc.DocumentNumber;
            ws.Cell(r, 17).Value = inc.DocumentDate?.ToString("yyyy-MM-dd");
            ws.Cell(r, 18).Value = inc.DocumentType;
            ws.Cell(r, 19).Value = inc.Email;
            ws.Cell(r, 20).Value = inc.ManagerId?.ToString();
            ws.Cell(r, 21).Value = inc.Status;
            ws.Cell(r, 22).Value = inc.Notes;
        }

        // File template người dùng tải về
        public byte[] BuildTemplate()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Template");
            string[] headers = new[]
            {
                "TaxAgencyCode","TaxAgencyName","TaxPayer","TaxCode","TaxPayerName","ManagementDept",
                "ManagerName","MainBusinessCode","MainBusinessName","BusinessDescription",
                "DirectorName","DirectorPhone","ChiefAccountant","ChiefAccountantPhone",
                "DocumentNumber","DocumentDate","DocumentType","Email","ManagerId","Status","Notes"
            };
            for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;
            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
