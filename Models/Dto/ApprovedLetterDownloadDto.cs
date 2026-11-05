using System.ComponentModel.DataAnnotations;

namespace Letter_Request_Management_System.Models.Dto
{
    public class ApprovedLetterDownloadDto
    {
        public string LetterContent { get; set; } = string.Empty;
        public string LetterTypeName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public byte[] SignatureImage { get; set; } = Array.Empty<byte>();
        public string ApproverName { get; set; } = string.Empty;
        public DateTime ApprovedAt { get; set; }
    }
}
