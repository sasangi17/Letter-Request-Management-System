using System.ComponentModel.DataAnnotations;

namespace Letter_Request_Management_System.Models.Dto
{
    public class LetterRequestDto
    {
        [Required]
        public string LetterTypeName { get; set; } = string.Empty;
    }
}