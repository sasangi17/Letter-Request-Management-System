using Letter_Request_Management_System.Models.Domain;

namespace Letter_Request_Management_System.LetterTemplates
{
    public interface ILetterTemplate
    {
        string GenerateLetter(UserMl employee);
    }
}
