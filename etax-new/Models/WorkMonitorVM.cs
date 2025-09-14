using System;
using System.Collections.Generic;
using prjetax.Models;

namespace prjetax.Models
{
    public class WorkMonitorVM
    {
        // Bộ lọc
        public string? Q { get; set; }
        public WorkStatus? S { get; set; }
        public int? ManagerId { get; set; }
        public int? EnterpriseId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        // Số liệu tổng quan
        public int Total { get; set; }
        public int Doing { get; set; }
        public int Done { get; set; }
        public int Overdue { get; set; }
        public int DueSoon { get; set; } // T+2

        // Dữ liệu hiển thị
        public List<WorkItem> Items { get; set; } = new();
    }
}
