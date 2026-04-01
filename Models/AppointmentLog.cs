namespace ChairmanOMS.Models
{
    public class AppointmentLog
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public string Status { get; set; } = string.Empty;
        public string? ChangedById { get; set; }
        public ApplicationUser? ChangedBy { get; set; }
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
