namespace prjetax.Models
{
    public class ExcelEnterpriseRow
    {
        public string TaxAgencyCode { get; set; } = "";
        public string TaxAgencyName { get; set; } = "";
        public string TaxPayer { get; set; } = "";
        public string TaxCode { get; set; } = "";            // Mã số thuế (khóa trùng)
        public string TaxPayerName { get; set; } = "";
        public string ManagementDept { get; set; } = "";
        public string ManagerName { get; set; } = "";
        public string MainBusinessCode { get; set; } = "";
        public string MainBusinessName { get; set; } = "";
        public string BusinessDescription { get; set; } = "";
        public string DirectorName { get; set; } = "";
        public string DirectorPhone { get; set; } = "";
        public string ChiefAccountant { get; set; } = "";
        public string ChiefAccountantPhone { get; set; } = "";
        public string DocumentNumber { get; set; } = "";
        public DateTime? DocumentDate { get; set; }
        public string DocumentType { get; set; } = "";
        public string Email { get; set; } = "";
        public int? ManagerId { get; set; }
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
    }
}
