using System.ComponentModel.DataAnnotations;

namespace ChairmanOMS.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
    }
}
