using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace prjetax.Models
{
    public class SendEmailViewModel
    {
        [Display(Name = "Gửi đến")]
        public List<string> ToList { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }
    }
}
