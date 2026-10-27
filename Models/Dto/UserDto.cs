namespace Letter_Request_Management_System.Models.Dto
{
    public class UserDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string EmpNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }
}
