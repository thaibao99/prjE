using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace prjetax.Models
{
    public class SendEmailViewModel
    {
        [Required(ErrorMessage = "Bạn phải nhập Email nhận")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email nhận")]
        public string ToEmail { get; set; } = default!;

        [Required(ErrorMessage = "Bạn phải nhập Chủ đề")]
        [Display(Name = "Tiêu đề mail")]
        public string Subject { get; set; } = default!;

        [Required(ErrorMessage = "Bạn phải nhập Nội dung mail")]
        [Display(Name = "Nội dung mail")]
        public string Body { get; set; } = default!;

        [Display(Name = "Ngày deadline")]
        public DateTime? Deadline { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Đánh giá DN")]
        public string? Rating { get; set; }

        [Display(Name = "Tệp đính kèm (PDF)")]
        public IFormFile? Attachment { get; set; }
        public string? DocumentName { get; set; }
    }
}
