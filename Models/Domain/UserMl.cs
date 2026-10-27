namespace Letter_Request_Management_System.Models.Domain
{
    public class UserMl
    {
        public int UserId { get; set; }
        public string EmpNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
        public DateTime? JoinDate { get; set; }
        public string? MobileNo { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
