using Letter_Request_Management_System.Models.Domain;
using QuestPDF.Fluent;

namespace Letter_Request_Management_System.Utility
{
    public static class SignatureSectionUtility
    {
        public static void AddSignatureSection(
            ColumnDescriptor column,
            AdminSignatureMl adminSignature,
            string adminName,
            DateTime approvedAt)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (adminSignature == null ||
                adminSignature.SignatureImage == null ||
                adminSignature.SignatureImage.Length == 0)
            {
                throw new InvalidOperationException(
                    "Admin signature is not available.");
            }

            if (string.IsNullOrWhiteSpace(adminName))
                throw new ArgumentException(
                    "Admin name is required.", nameof(adminName));

            column.Item()
                .PaddingTop(30)
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
    }
}