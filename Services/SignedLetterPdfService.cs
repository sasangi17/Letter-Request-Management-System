using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Letter_Request_Management_System.Services
{
    public class SignedLetterPdfService
    {
        private readonly string _pdfFolder;

        public SignedLetterPdfService(IWebHostEnvironment environment)
        {
            _pdfFolder = Path.Combine(
                environment.ContentRootPath,
                "App_Data",
                "SignedLetters");

            Directory.CreateDirectory(_pdfFolder);
        }

        // Generate a PDF in memory for direct downloading.
        public byte[] GenerateSignedPdfBytes(
            string letterContent,
            string approverName,
            DateTime approvedAt,
            byte[] signatureImage)
        {
            ValidateInputs(
                letterContent,
                approverName,
                signatureImage);

            return CreateDocument(
                letterContent,
                approverName,
                approvedAt,
                signatureImage).GeneratePdf();
        }

        // Generate and save a signed PDF.
        // Returns the relative PDF reference for database storage.
        public string GenerateSignedPdf(
            string letterContent,
            string approverName,
            DateTime approvedAt,
            byte[] signatureImage)
        {
            ValidateInputs(letterContent,approverName,signatureImage);

            string fileName = $"{Guid.NewGuid():N}.pdf";

            string fullPath = Path.Combine(_pdfFolder,fileName);

            CreateDocument(
                letterContent,
                approverName,
                approvedAt,
                signatureImage).GeneratePdf(fullPath);

            return Path.Combine(
                "App_Data",
                "SignedLetters",
                fileName).Replace("\\", "/");
        }

        // Validate the PDF inputs.
        private static void ValidateInputs(
            string letterContent,
            string approverName,
            byte[] signatureImage)
        {
            if (string.IsNullOrWhiteSpace(letterContent))
            {
                throw new ArgumentException("Letter content is required.");
            }

            if (string.IsNullOrWhiteSpace(approverName))
            {
                throw new ArgumentException("Approver name is required.");
            }

            if (signatureImage == null ||
                signatureImage.Length == 0)
            {
                throw new ArgumentException("Signature image is required.");
            }
        }

        // Create the QuestPDF document.
        private static IDocument CreateDocument(
            string letterContent,
            string approverName,
            DateTime approvedAt,
            byte[] signatureImage)
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(45);

                    page.DefaultTextStyle(style => style.FontSize(12));

                    page.Content().Column(column =>
                    {
                        column.Spacing(18);

                        column.Item().Text(letterContent);
                        column.Item().PaddingTop(20).Text("Approved by:");
                        column.Item().Text(approverName).Bold();
                        column.Item().Text("Authorized signature:");
                        column.Item().Width(180).Height(80).Image(signatureImage).FitArea();
                        column.Item().Text($"Approval date: {approvedAt:dd MMMM yyyy HH:mm}").FontSize(10);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Employee Letter Request Management System")
                        .FontSize(9);
                });
            });
        }
    }
}
