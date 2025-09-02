using System.Collections.Generic;

namespace prjetax.Models
{
    public class DashboardViewModel
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public List<EnterpriseDemo> Enterprises { get; set; }
    }
}
