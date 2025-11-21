using System;

namespace ASI.Basecode.Data.Models
{
    public partial class Ticket
    {
        public int Id { get; set; }
        public string Summary { get; set; } = null!;
        public int UserID { get; set; }
        public int AgentID { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ResolvedAt { get; set; }
        public string Type { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; } = null!;
        public string Category { get; set; } = null!;
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public string? AttachmentPath { get; set; }

    }
}