using System.ComponentModel.DataAnnotations;

namespace ChairmanOMS.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public string VisitorName { get; set; } = string.Empty;

        [Required]
        public string Organization { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }

        [Required]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public string? Masuulka { get; set; }

        public string Priority { get; set; } = "Normal"; // Normal, Urgent

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed, Cancelled

        public string? AttachmentPath { get; set; }

        // Visitor check-in
        public string? VisitorStatus { get; set; } // Arrived, No-show
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<AppointmentLog> Logs { get; set; } = new List<AppointmentLog>();
        public ICollection<MeetingNote> MeetingNotes { get; set; } = new List<MeetingNote>();
    }
}
