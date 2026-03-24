using System.ComponentModel.DataAnnotations;

namespace ChairmanOMS.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser? CreatedBy { get; set; }

        public string? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }
    }
}
