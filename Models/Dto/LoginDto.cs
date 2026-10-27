using System.ComponentModel.DataAnnotations;

namespace Letter_Request_Management_System.Models.Dto
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
        public string? SelectedRole { get; set; }
    }
}
