using System.ComponentModel.DataAnnotations;

namespace ChairmanOMS.Models
{
    public class IncomingDocument
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string SourceInstitution { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime DateReceived { get; set; }
        public string Priority { get; set; } = "Medium"; // Low, Medium, High
        public string? AttachmentPath { get; set; }
        public string? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }
        public string Status { get; set; } = "Received"; // Received, UnderReview, Approved, Rejected, Archived
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<WorkflowAction> WorkflowActions { get; set; } = new List<WorkflowAction>();
    }
}
