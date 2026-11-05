using Letter_Request_Management_System.Models.Domain;
using Letter_Request_Management_System.Utility;

namespace Letter_Request_Management_System.LetterTemplates
{
    public class ServiceLetterTemplate : ILetterTemplate
    {
        public string GenerateLetter(UserMl employee)
        {
            return $@"
Service Letter

Date: {DateTimeUtility.FormatLetterDate(DateTime.Now)}



This is to certify that {employee.FullName}, bearing Employee Number
{employee.EmpNo}, is employed at our organization.The employee works in the {employee.Department} department as a
{employee.Position}.The employee joined the organization on
{DateTimeUtility.FormatJoinDate(employee.JoinDate)}.

This letter is issued upon the employee's request for official purposes.

Authorized Signature
Human Resources Department
";
        }
    }
}