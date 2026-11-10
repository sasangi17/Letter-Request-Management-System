using Letter_Request_Management_System.Data.Interface;
using Letter_Request_Management_System.Models.Domain;
using Letter_Request_Management_System.Utilities;
using Letter_Request_Management_System.Utility;
using QuestPDF.Fluent;
using QuestPDF.Helpers;


namespace Letter_Request_Management_System.LetterTemplates
{
    public class SalaryConfirmationLetterTemplate : ILetterTemplate
    {
        // Generate letter content for employee request creation.
        public string GenerateLetter(UserMl employee)
        {
            string companyAddress = LetterAddressUtility.GetCompanyAddress();
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (!employee.Salary.HasValue)
                throw new InvalidOperationException(
                    "Salary information is not available for this employee.");
            return $@"
{companyAddress}

SALARY CONFIRMATION LETTER

Date: {DateTimeUtility.FormatLetterDate(DateTime.Now)}

This is to certify that {employee.FullName}, bearing Employee Number {employee.EmpNo}, is currently employed at our organization.

The employee works in the {employee.Department} department as a {employee.Position}.

The employee's monthly salary is {employee.Salary.Value:N2}.

This letter is issued upon the employee's request for official purposes.";
        }

        // Generate the final signed salary confirmation PDF.
        public byte[] GeneratePdf(
            UserMl employee,
            AdminSignatureMl adminSignature,
            string adminName,
            DateTime approvedAt)
        {
            ValidateInputs(employee, adminSignature, adminName);

            if (!employee.Salary.HasValue)
                throw new InvalidOperationException("Salary information is not available for this employee.");

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(45);

                    page.DefaultTextStyle(style =>
                        style.FontSize(12));

                    page.Content().Column(column =>
                    {
                        column.Spacing(18);

                        column.Item()
                            .AlignCenter()
                            .Text("SALARY CONFIRMATION LETTER")
                            .FontSize(18)
                            .Bold();

                        column.Item()
                            .AlignRight()
                            .Text($"Date: {DateTimeUtility.FormatLetterDate(approvedAt)}");

                        column.Item().Text(
                            $"This is to certify that {employee.FullName}, " +
                            $"bearing Employee Number {employee.EmpNo}, " +
                            "is currently employed at our organization.");

                        column.Item().Text(
                            $"The employee works in the {employee.Department} " +
                            $"department as a {employee.Position}.");

                        column.Item().Text(
                            $"The employee's monthly salary is " +
                            $"{employee.Salary.Value:N2}.");

                        column.Item().Text(
                            "This letter is issued upon the employee's request " +
                            "for official purposes.");

                        AddSignatureSection(column, adminSignature, adminName, approvedAt);
                    });

                    page.Footer().AlignCenter().Text("Employee Letter Request Management System").FontSize(9);
                });
            }).GeneratePdf();
        }

        private static void AddSignatureSection(
            ColumnDescriptor column,
            AdminSignatureMl adminSignature,
            string adminName,
            DateTime approvedAt)
        {
            column.Item().PaddingTop(30).Text("Authorized Signature");
            column.Item().Width(180).Height(80).Image(adminSignature.SignatureImage).FitArea();
            column.Item().Text(adminName).Bold();
            column.Item().Text($"Approval Date: {approvedAt:dd MMMM yyyy}");
            column.Item().Text("Human Resources Department");
        }

        private static void ValidateInputs(
            UserMl employee,
            AdminSignatureMl adminSignature,
            string adminName)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (adminSignature == null ||
                adminSignature.SignatureImage == null ||
                adminSignature.SignatureImage.Length == 0)
            {
                throw new InvalidOperationException("Admin signature is not available.");
            }

            if (string.IsNullOrWhiteSpace(adminName))
                throw new ArgumentException("Admin name is required.", nameof(adminName));
        }
    }
}