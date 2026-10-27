namespace Letter_Request_Management_System.Models.Dto
{
    public class LetterRequestResponseDto
    {
        public int LetterRequestId { get; set; }
        public int LetterTypeId { get; set; }
        public string LetterTypeName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string LetterContent { get; set; } = string.Empty;
    }
}
