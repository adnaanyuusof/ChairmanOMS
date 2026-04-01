namespace ChairmanOMS.Models
{
    public class MeetingNote
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public string Notes { get; set; } = string.Empty;
        public string? Decision { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
    }
}
