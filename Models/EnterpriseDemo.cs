using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjetax.Models
{
    [Table("EnterprisesDemo")]
    public class EnterpriseDemo
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)] public string TaxAgencyCode { get; set; }
        [StringLength(200)] public string TaxAgencyName { get; set; }
        [StringLength(200)] public string TaxPayer { get; set; }
        [StringLength(50)] public string TaxCode { get; set; }
        [StringLength(500)] public string TaxPayerName { get; set; }
        [StringLength(200)] public string ManagementDept { get; set; }
        [StringLength(100)] public string ManagerName { get; set; }
        [StringLength(50)] public string MainBusinessCode { get; set; }
        [StringLength(200)] public string MainBusinessName { get; set; }
        public string BusinessDescription { get; set; }
        [StringLength(200)] public string DirectorName { get; set; }
        [StringLength(50)] public string DirectorPhone { get; set; }
        [StringLength(200)] public string ChiefAccountant { get; set; }
        [StringLength(50)] public string ChiefAccountantPhone { get; set; }
        [StringLength(100)] public string DocumentNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DocumentDate { get; set; }

        [StringLength(100)] public string DocumentType { get; set; }
        [StringLength(200)] public string Email { get; set; }

        // FK → Managers(Id)
        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public Manager Manager { get; set; }

        [StringLength(50)] public string Status { get; set; }
        public string Notes { get; set; }
    }
}
