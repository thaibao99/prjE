using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjetax.Models
{
    [Table("EnterpriseHistories")]
    public class EnterpriseHistory
    {
        [Key]
        public int Id { get; set; }
        public int EnterpriseId { get; set; }

        [ForeignKey(nameof(EnterpriseId))]
        public EnterpriseDemo Enterprise { get; set; }

        public DateTime Date { get; set; }
        public string Document { get; set; }
        public string Content { get; set; }
        public DateTime? Reminder { get; set; }
        public string Result { get; set; }
        public string Notes { get; set; }
        public string Rating { get; set; }
    }
}
