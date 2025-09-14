namespace prjetax.Models
{
    public class WorkLog
    {
        public int Id { get; set; }

        public int WorkItemId { get; set; }
        public WorkItem WorkItem { get; set; } = default!;

        public DateTime At { get; set; } = DateTime.Now;
        public string Content { get; set; } = string.Empty;
        public string? Result { get; set; }

        // đính kèm nhỏ (PDF/JPG)
        public byte[]? Attachment { get; set; }
        public string? AttachmentName { get; set; }
    }
}
