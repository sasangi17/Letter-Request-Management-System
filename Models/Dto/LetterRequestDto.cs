using System.ComponentModel.DataAnnotations;

namespace Letter_Request_Management_System.Models.Dto
{
    public class LetterRequestDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int LetterTypeId { get; set; }
    }
}