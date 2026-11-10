using Letter_Request_Management_System.Models.Domain;

namespace Letter_Request_Management_System.Data.Interface
{
    public interface ILetterTemplate
    {
        // Used when the employee creates a request.
        string GenerateLetter(UserMl employee);

        // Used when the admin approves the request.
        byte[] GeneratePdf(
            UserMl employee,
            AdminSignatureMl adminSignature,
            string adminName,
            DateTime approvedAt);
    }
}