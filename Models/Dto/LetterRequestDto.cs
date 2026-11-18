using System.ComponentModel.DataAnnotations;

namespace Letter_Request_Management_System.Models.Dto
{
    public class LetterRequestDto
    {
        [Required]
        public string LetterTypeName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
           "^(Soft Copy|Hard Copy)$",
           ErrorMessage = "Copy type must be Soft Copy or Hard Copy.")]
        public string CopyType { get; set; } = "Soft Copy";
    }
}