namespace Letter_Request_Management_System.Models.Dto
{
    public class AdminLetterRequestDto
    {
        public int LetterRequestId { get; set; }
        public int UserId { get; set; }
        public string EmpNo { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string LetterTypeName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string LetterContent { get; set; } = string.Empty;
    }
}
