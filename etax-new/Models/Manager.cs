using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjetax.Models
{
    [Table("Managers")]
    public class Manager
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; }

        [Required, StringLength(200)]
        public string PasswordHash { get; set; }

        [Column("FullName"), StringLength(200)]
        [Display(Name = "Tên nhân viên")]
        public string Name { get; set; }

        public bool IsAdmin { get; set; }

        public ICollection<EnterpriseDemo>? Enterprises { get; set; }
    }
}
