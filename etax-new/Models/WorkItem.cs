using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace prjetax.Models
{
    public class WorkItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public WorkStatus Status { get; set; } = WorkStatus.Todo;
        public DateTime? DueDate { get; set; }

        public int ManagerId { get; set; }

        [ValidateNever]         // <-- tránh validate navigation
        public Manager Manager { get; set; } = default!;

        public int? EnterpriseId { get; set; }

        [ValidateNever]         // <-- tránh validate navigation
        public EnterpriseDemo? Enterprise { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public ICollection<WorkLog> Logs { get; set; } = new List<WorkLog>();
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Status == WorkStatus.Done)
            {
                if (CompletedAt == null)
                    yield return new ValidationResult("Vui lòng nhập ngày hoàn thành.", new[] { nameof(CompletedAt) });

                if (CompletedAt != null && CompletedAt > DateTime.UtcNow.AddMinutes(1)) // cho phép lệch 1'
                    yield return new ValidationResult("Ngày hoàn thành không được lớn hơn hiện tại.", new[] { nameof(CompletedAt) });

                if (CompletedAt != null && CreatedAt > CompletedAt)
                    yield return new ValidationResult("Ngày hoàn thành không được nhỏ hơn ngày tạo.", new[] { nameof(CompletedAt) });
            }
        }
    }

    public enum WorkStatus : byte   // <-- thêm : byte
    {
        Todo = 0, Doing = 1, Done = 2, Blocked = 3
    }
}
