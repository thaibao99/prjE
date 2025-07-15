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

        [Required(ErrorMessage = "Vui lòng nhập Mã cơ quan thuế")]
        [StringLength(50)]
        public string TaxAgencyCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên cơ quan thuế")]
        [StringLength(200)]
        public string TaxAgencyName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Người nộp thuế")]
        [StringLength(200)]
        public string TaxPayer { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mã số thuế")]
        [StringLength(50)]
        public string TaxCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên doanh nghiệp")]
        [StringLength(500)]
        public string TaxPayerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Phòng quản lý")]
        [StringLength(200)]
        public string ManagementDept { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên cán bộ quản lý (ghi trên chứng từ)")]
        [StringLength(100)]
        public string ManagerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mã ngành nghề chính")]
        [StringLength(50)]
        public string MainBusinessCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Ngành nghề chính")]
        [StringLength(200)]
        public string MainBusinessName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mô tả hoạt động kinh doanh")]
        public string BusinessDescription { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên Giám đốc")]
        [StringLength(200)]
        public string DirectorName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại Giám đốc")]
        [StringLength(50)]
        public string DirectorPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên Kế toán trưởng")]
        [StringLength(200)]
        public string ChiefAccountant { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại Kế toán trưởng")]
        [StringLength(50)]
        public string ChiefAccountantPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số chứng từ")]
        [StringLength(100)]
        public string DocumentNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Ngày chứng từ")]
        [Column(TypeName = "date")]
        public DateTime? DocumentDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Loại chứng từ")]
        [StringLength(100)]
        public string DocumentType { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Cán bộ phụ trách")]
        public int? ManagerId { get; set; }

        [ForeignKey(nameof(ManagerId))]
        public Manager Manager { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Trạng thái")]
        [StringLength(50)]
        public string Status { get; set; }

        public string Notes { get; set; }
    }
}
