namespace ChairmanOMS.Models
{
    public class WorkflowAction
    {
        public int Id { get; set; }
        public string ActionTaken { get; set; } = string.Empty; // Approved, Rejected, Commented
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string DocumentType { get; set; } = string.Empty; // Incoming, Outgoing
        public int? IncomingDocumentId { get; set; }
        public IncomingDocument? IncomingDocument { get; set; }
        
        public int? OutgoingDocumentId { get; set; }
        public OutgoingDocument? OutgoingDocument { get; set; }
    }
}
