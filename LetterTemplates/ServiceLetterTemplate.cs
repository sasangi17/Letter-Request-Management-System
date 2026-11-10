using Letter_Request_Management_System.Data.Interface;
using Letter_Request_Management_System.Models.Domain;
using Letter_Request_Management_System.Utilities;
using Letter_Request_Management_System.Utility;
using QuestPDF.Fluent;
using QuestPDF.Helpers;


namespace Letter_Request_Management_System.LetterTemplates
{
    public class ServiceLetterTemplate : ILetterTemplate
    {
        // Generate letter content for employee request creation.
        public string GenerateLetter(UserMl employee)
        {
            string companyAddress = LetterAddressUtility.GetCompanyAddress();
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            return $@"
{companyAddress}

SERVICE LETTER

Date: {DateTimeUtility.FormatLetterDate(DateTime.Now)}

This is to certify that {employee.FullName}, bearing Employee Number {employee.EmpNo}, has been employed at our organization.

The employee is currently working in the {employee.Department} department as a {employee.Position}.

The employee joined the organization on {(employee.JoinDate.HasValue ? DateTimeUtility.FormatLetterDate(employee.JoinDate.Value) : "N/A")}.

This letter is issued upon the employee's request for official purposes.";
        }

        // Generate the final signed service letter PDF.
        public byte[] GeneratePdf(
            UserMl employee,
            AdminSignatureMl adminSignature,
            string adminName,
            DateTime approvedAt)
        {
            ValidateInputs(employee, adminSignature, adminName);

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
                            .Text("SERVICE LETTER")
                            .FontSize(18)
                            .Bold();

                        column.Item()
                            .AlignRight()
                            .Text($"Date: {DateTimeUtility.FormatLetterDate(approvedAt)}");

                        column.Item().Text(
                            $"This is to certify that {employee.FullName}, " +
                            $"bearing Employee Number {employee.EmpNo}, " +
                            "has been employed at our organization.");

                        column.Item().Text(
                            $"The employee is currently working in the " +
                            $"{employee.Department} department as a {employee.Position}.");

                        column.Item().Text(
                            $"The employee joined the organization on " +
                            $"{(employee.JoinDate.HasValue ? DateTimeUtility.FormatLetterDate(employee.JoinDate.Value) : "N/A")}.");

                        column.Item().Text(
                            "This letter is issued upon the employee's request " +
                            "for official purposes.");

                        AddSignatureSection(column, adminSignature, adminName, approvedAt);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Employee Letter Request Management System")
                        .FontSize(9);
                });
            }).GeneratePdf();
        }

        private static void AddSignatureSection(
            ColumnDescriptor column,
            AdminSignatureMl adminSignature,
            string adminName,
            DateTime approvedAt)
        {
            column.Item().PaddingTop(30)
                .Text("Authorized Signature");

            column.Item()
                .Width(180)
                .Height(80)
                .Image(adminSignature.SignatureImage)
                .FitArea();

            column.Item()
                .Text(adminName)
                .Bold();

            column.Item()
                .Text($"Approval Date: {approvedAt:dd MMMM yyyy}");

            column.Item()
                .Text("Human Resources Department");
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
                throw new InvalidOperationException(
                    "Admin signature is not available.");
            }

            if (string.IsNullOrWhiteSpace(adminName))
                throw new ArgumentException(
                    "Admin name is required.",
                    nameof(adminName));
        }
    }
}