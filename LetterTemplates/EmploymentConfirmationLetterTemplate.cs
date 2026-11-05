using Letter_Request_Management_System.Models.Domain;
using Letter_Request_Management_System.Utility;

namespace Letter_Request_Management_System.LetterTemplates
{
    public class EmploymentConfirmationLetterTemplate : ILetterTemplate
    {
        public string GenerateLetter(UserMl employee)
        {
            return $@"
Employment Confirmation Letter

Date: {DateTimeUtility.FormatLetterDate(DateTime.Now)}



This letter confirms that {employee.FullName}, Employee Number{employee.EmpNo}, is currently employed at our organization.
The employee is working as a {employee.Position} in the{employee.Department} department.

The employee's date of joining is{DateTimeUtility.FormatJoinDate(employee.JoinDate)}.

This confirmation is issued upon the employee's request.

Authorized Signature
Human Resources Department
";
        }
    }
}