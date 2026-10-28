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
            string signatureImagePath)
        {
            ValidateInputs(
                letterContent,
                approverName,
                signatureImagePath);

            return CreateDocument(
                letterContent,
                approverName,
                approvedAt,
                signatureImagePath).GeneratePdf();
        }

        // Retained for compatibility with existing approval code.
        public string GenerateSignedPdf(
            string letterContent,
            string approverName,
            DateTime approvedAt,
            string signatureImagePath)
        {
            ValidateInputs(
                letterContent,
                approverName,
                signatureImagePath);

            string fileName = $"{Guid.NewGuid():N}.pdf";
            string fullPath = Path.Combine(_pdfFolder, fileName);

            CreateDocument(
                letterContent,
                approverName,
                approvedAt,
                signatureImagePath).GeneratePdf(fullPath);

            return Path.Combine(
                "App_Data",
                "SignedLetters",
                fileName).Replace("\\", "/");
        }

        private static void ValidateInputs(
            string letterContent,
            string approverName,
            string signatureImagePath)
        {
            if (string.IsNullOrWhiteSpace(letterContent))
                throw new ArgumentException("Letter content is required.");

            if (string.IsNullOrWhiteSpace(approverName))
                throw new ArgumentException("Approver name is required.");

            if (string.IsNullOrWhiteSpace(signatureImagePath) ||
                !File.Exists(signatureImagePath))
            {
                throw new FileNotFoundException("Signature image was not found.",signatureImagePath);
            }
        }

        private static QuestPDF.Infrastructure.IDocument CreateDocument(
            string letterContent,
            string approverName,
            DateTime approvedAt,
            string signatureImagePath)
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

                        column.Item().Text("EMPLOYEE LETTER").FontSize(20).Bold();
                        column.Item().Text(letterContent);
                        column.Item().PaddingTop(20).Text("Approved by:");
                        column.Item().Text(approverName).Bold();
                        column.Item().Text("Authorized signature:");
                        column.Item().Width(180).Height(80).Image(signatureImagePath).FitArea();
                        column.Item().Text($"Approval date: {approvedAt:dd MMMM yyyy HH:mm}").FontSize(10);
                    });

                    page.Footer().AlignCenter().Text("Employee Letter Request Management System").FontSize(9);
                });
            });
        }
    }
}
