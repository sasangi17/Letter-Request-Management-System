
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Letter_Request_Management_System.Services
{
    public class PreviewLetterPdfService
    {
        public byte[] GeneratePreviewPdfBytes(string letterContent)
        {
            if (string.IsNullOrWhiteSpace(letterContent))
            {
                throw new ArgumentException("Letter content is required.");
            }

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(45);
                    page.DefaultTextStyle(style => style.FontSize(12));

                    page.Content().Layers(layers =>
                    {
                        // Main letter content
                        layers.PrimaryLayer().Column(column =>
                        {
                            column.Spacing(18);

                            column.Item()
                                .Text("EMPLOYEE LETTER")
                                .FontSize(20)
                                .Bold();

                            column.Item().Text(letterContent);
                        });

                        // Watermark
                        layers.Layer()
                            .AlignCenter()
                            .AlignMiddle()
                            .Rotate(-35)
                            .Text("NOT APPROVED")
                            .FontSize(48)
                            .Bold()
                            .FontColor(Colors.Grey.Lighten2);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("UNOFFICIAL PREVIEW - NOT APPROVED")
                        .FontSize(9);
                });
            }).GeneratePdf();
        }
    }
}
