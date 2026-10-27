using System.ComponentModel.DataAnnotations;

namespace Letter_Request_Management_System.Models.Dto
{
    public class ApproveLetterRequestDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string SignatureReference { get; set; } = string.Empty;
    }
}
