using System.ComponentModel.DataAnnotations;

namespace ChairmanOMS.Models
{
    public class OutgoingDocument
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string DestinationInstitution { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime DateSent { get; set; }
        public string? AttachmentPath { get; set; }
        public string? SignaturePath { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Approved, Sent, Delivered
        
        public int? LinkedIncomingDocumentId { get; set; }
        public IncomingDocument? LinkedIncomingDocument { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<WorkflowAction> WorkflowActions { get; set; } = new List<WorkflowAction>();
    }
}
