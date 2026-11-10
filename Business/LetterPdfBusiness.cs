using Letter_Request_Management_System.Business;
using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Dto;
using Letter_Request_Management_System.Services;

namespace Letter_Request_Management_System.Business
{
    public class LetterPdfBusiness
    {
        private readonly LetterRequestRepository _letterRequestRepository;
        private readonly AdminSignatureBusiness _adminSignatureBusiness;
        private readonly SignedLetterPdfService _signedLetterPdfService;
        private readonly PreviewLetterPdfService _previewLetterPdfService;

        public LetterPdfBusiness(
            LetterRequestRepository letterRequestRepository,
            AdminSignatureBusiness adminSignatureBusiness,
            SignedLetterPdfService signedLetterPdfService,
            PreviewLetterPdfService previewLetterPdfService)
        {
            _letterRequestRepository = letterRequestRepository;
            _adminSignatureBusiness = adminSignatureBusiness;
            _signedLetterPdfService = signedLetterPdfService;
            _previewLetterPdfService = previewLetterPdfService;
        }

        // Generate an unofficial PDF preview for a pending employee request.
        public LetterRequestResponseDto? GetPreviewLetter(int letterRequestId, int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
                return null;

            var request = _letterRequestRepository.GetRequestById(letterRequestId, userId);

            if (request == null ||
                !string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return request;
        }

        // Generate and save the signed PDF when an admin approves a request.
        // AdminBusiness should call this method before calling repository ApproveRequest.
        public string? GenerateSignedPdfForApproval(int letterRequestId, int approvedByUserId)
        {
            if (letterRequestId <= 0 || approvedByUserId <= 0)
                return null;

            var request = _letterRequestRepository.GetAdminRequestById(letterRequestId);

            if (request == null)
                return null;

            if (!string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Retrieve the signature belonging to the approving admin.
            var signature = _adminSignatureBusiness.GetSignature(approvedByUserId);

            if (signature == null ||
                signature.SignatureImage == null ||
                signature.SignatureImage.Length == 0)
            {
                return null;
            }

            var adminName = _letterRequestRepository.GetUserFullName(approvedByUserId);

            if (string.IsNullOrWhiteSpace(adminName))
                return null;

            return _signedLetterPdfService.GenerateSignedPdf(
                request.LetterContent,
                adminName,
                DateTime.Now,
                signature.SignatureImage);
        }

        public string GenerateSignedPdf(string letterContent, string approverName, DateTime approvedAt, byte[] signatureImage)
        {
            return _signedLetterPdfService.GenerateSignedPdf(
                letterContent,
                approverName,
                approvedAt,
                signatureImage);
        }

        // Retrieve the approved PDF content for the employee who owns the request.
        // Retrieve the approved PDF content and a download file name
        // (requested user name + letter type name).
        public (byte[] PdfBytes, string FileName)? GetApprovedPdfForDownload(int letterRequestId, int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
                return null;

            var approvedLetter = _letterRequestRepository.GetApprovedLetterForDownload(letterRequestId, userId);

            if (approvedLetter == null ||
                string.IsNullOrWhiteSpace(approvedLetter.LetterContent) ||
                approvedLetter.SignatureImage == null ||
                approvedLetter.SignatureImage.Length == 0)
            {
                return null;
            }

            var pdfBytes = _signedLetterPdfService.GenerateSignedPdfBytes(
                approvedLetter.LetterContent,
                approvedLetter.ApproverName,
                approvedLetter.ApprovedAt,
                approvedLetter.SignatureImage);

            string fileName = BuildDownloadFileName(
                approvedLetter.EmployeeName,
                approvedLetter.LetterTypeName,
                letterRequestId);

            return (pdfBytes, fileName);
        }

        // Builds a safe file name like "John_Doe_Service_Letter.pdf"
        private static string BuildDownloadFileName(string employeeName, string letterTypeName, int letterRequestId)
        {
            string combined = $"{employeeName} {letterTypeName}".Trim();

            // Remove characters that are invalid in file names
            foreach (char c in Path.GetInvalidFileNameChars())
                combined = combined.Replace(c.ToString(), string.Empty);

            // Collapse whitespace into single underscores
            combined = System.Text.RegularExpressions.Regex.Replace(combined, @"\s+", "_");

            // Fallback if the name ended up empty
            if (string.IsNullOrWhiteSpace(combined))
                combined = $"Approved_Letter_{letterRequestId}";

            return $"{combined}.pdf";
        }
    }
}