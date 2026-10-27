namespace Letter_Request_Management_System.Models.Domain
{
    public class LetterRequestMl
    {
        public int LetterRequestId { get; set; }
        public int UserId { get; set; }
        public int LetterTypeId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string LetterContentSnapshot { get; set; } = string.Empty;
    }
}
