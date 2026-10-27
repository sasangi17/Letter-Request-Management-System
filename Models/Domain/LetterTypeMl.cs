namespace Letter_Request_Management_System.Models.Domain
{
    public class LetterTypeMl
    {
        public int LetterTypeId { get; set; }
        public string LetterTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TemplateContent { get; set; } = string.Empty;
    }
}
