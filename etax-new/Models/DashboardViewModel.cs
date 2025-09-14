using System.Collections.Generic;

namespace prjetax.Models
{
    public class DashboardViewModel
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public List<EnterpriseDemo> Enterprises { get; set; }
        public int TotalWork { get; set; }
        public int Doing { get; set; }
        public int Overdue { get; set; }
        public int Done { get; set; }
        public IEnumerable<WorkItem>? DueSoon { get; set; }
        public IEnumerable<WorkItem>? OverdueList { get; set; }
    }
}
