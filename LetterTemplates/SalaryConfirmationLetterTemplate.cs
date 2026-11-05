using Letter_Request_Management_System.Models.Domain;
using Letter_Request_Management_System.Utility;

namespace Letter_Request_Management_System.LetterTemplates
{
    public class SalaryConfirmationLetterTemplate : ILetterTemplate
    {
        public string GenerateLetter(UserMl employee)
        {
            if (!employee.Salary.HasValue)
            {
                throw new InvalidOperationException(
                    "Salary information is not available for this employee.");
            }

            string formattedSalary = employee.Salary.Value.ToString("N2");

            return $@"
Salary Confirmation Letter

Date: {DateTimeUtility.FormatLetterDate(DateTime.Now)}


This letter confirms that {employee.FullName}, bearing Employee Number {employee.EmpNo}, is employed at our organization.The employee works as a {employee.Position} in the{employee.Department} department.

The employee's current monthly salary is LKR {formattedSalary}.

This letter is issued upon the employee's request for official purposes.

Authorized Signature
Human Resources Department
";
        }
    }
}