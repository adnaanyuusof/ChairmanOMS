using System.ComponentModel.DataAnnotations;

namespace ChairmanOMS.Models
{
    public class ApprovedDocument
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Reference Number")]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = string.Empty; // Incoming, Outgoing

        [Display(Name = "Source / Destination")]
        public string SourceOrDestination { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Display(Name = "Approved Date")]
        public DateTime ApprovedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Approved By")]
        public string? ApprovedByUserId { get; set; }
        public ApplicationUser? ApprovedBy { get; set; }

        public int? RelatedDocumentId { get; set; }

        [Display(Name = "Attachment")]
        public string? FilePath { get; set; }

        public string Status { get; set; } = "Approved"; // Approved, Archived

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
    }
}
