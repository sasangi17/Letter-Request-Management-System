namespace Letter_Request_Management_System.Models.Domain
{
    public class AdminSignatureMl
    {
        public int AdminSignatureId { get; set; }
        public int AdminUserId { get; set; }
        public byte[] SignatureImage { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
