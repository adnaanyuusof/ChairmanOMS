namespace ChairmanOMS.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty; // Create, Update, Delete, Login, etc.
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}
