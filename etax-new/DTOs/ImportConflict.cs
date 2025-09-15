namespace prjetax.Models
{
    public class ImportConflict
    {
        public ExcelEnterpriseRow Incoming { get; set; } = new();
        public EnterpriseDemo Existing { get; set; } = null!;
        public bool Overwrite { get; set; } = false; // User tick ghi đè
    }

    public class ImportPreviewVM
    {
        public List<ExcelEnterpriseRow> NewItems { get; set; } = new();
        public List<ImportConflict> Conflicts { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string TempToken { get; set; } = ""; // lưu state giữa Upload -> Preview -> Confirm
    }
}
