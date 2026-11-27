using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Dto;


namespace Letter_Request_Management_System.Business
{
    public class AdminBusiness
    {
        private readonly LetterRequestRepository _letterRequestRepository;
        private readonly AdminSignatureBusiness _adminSignatureBusiness;
        private readonly LetterPdfBusiness _letterPdfBusiness;

        public AdminBusiness(
            LetterRequestRepository letterRequestRepository,
            AdminSignatureBusiness adminSignatureBusiness,
            LetterPdfBusiness letterPdfBusiness)
        {
            _letterRequestRepository = letterRequestRepository;
            _adminSignatureBusiness = adminSignatureBusiness;
            _letterPdfBusiness = letterPdfBusiness;
        }

        public List<AdminLetterRequestDto> GetAllRequests()
        {
            return _letterRequestRepository.GetAllRequests();
        }

        public AdminLetterRequestDto? GetRequestById(int letterRequestId)
        {
            return _letterRequestRepository.GetAdminRequestById(letterRequestId);
        }

        public (byte[] PdfBytes, string FileName)? GetApprovedPdfForAdminDownload(int letterRequestId)
        {
            return _letterPdfBusiness.GetApprovedPdfForAdminDownload(letterRequestId);
        }

        public int RejectRequest(int letterRequestId,int rejectedByUserId,string rejectionReason)
        {
            // Validate input.
            if (letterRequestId <= 0 || rejectedByUserId <= 0)
                return 0;

            if (string.IsNullOrWhiteSpace(rejectionReason))
                return 6;

            if (rejectionReason.Length > 1000)
                return 7;

            // Check whether the request exists.
            var request =
                _letterRequestRepository.GetAdminRequestById(
                    letterRequestId);

            if (request == null)
                return 0;

            // Check whether the request is Pending.
            if (!string.Equals(
                request.Status,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            // Save rejection details.
            return _letterRequestRepository.RejectRequest(
                letterRequestId,
                rejectedByUserId,
                rejectionReason.Trim());
        }

        public int MarkRequestAsPrinted(int letterRequestId, int printedByUserId)
        {
            if (letterRequestId <= 0 || printedByUserId <= 0)
                return 0;

            // Get the request.
            var request = _letterRequestRepository.GetAdminRequestById(
                letterRequestId);

            if (request == null)
                return 0;

            // Only approved requests can be printed.
            if (!string.Equals(
                request.Status,
                "Approved",
                StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            // Only Hard Copy requests use the HR collection process.
            if (!string.Equals(
                request.CopyType,
                "Hard Copy",
                StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            // Avoid marking an already printed request again.
            if (request.IsPrinted)
                return 4;

            return _letterRequestRepository.MarkAsPrinted(
                letterRequestId,
                printedByUserId);
        }

        public int ApproveRequest(int letterRequestId,int approvedByUserId)
        {
            if (letterRequestId <= 0 || approvedByUserId <= 0)
                return 0;

            // Get the employee's request.
            var request =_letterRequestRepository.GetAdminRequestById(letterRequestId);

            if (request == null)
                return 0;

            // Check request status.
            if (!string.Equals(
                request.Status,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            // Get the admin's saved signature.
            var signature =_adminSignatureBusiness.GetSignature(approvedByUserId);

            if (signature == null)
                return 3;

            if (signature.SignatureImage == null ||
                signature.SignatureImage.Length == 0)
            {
                return 4;
            }

            // Get the admin's full name.
            string? adminName = _letterRequestRepository.GetUserFullName(approvedByUserId);

            if (string.IsNullOrWhiteSpace(adminName))
                return 5;

            // Generate the signed PDF.
            DateTime approvedAt = DateTime.Now;

            string pdfReference =
                _letterPdfBusiness.GenerateSignedPdf(
                    request.LetterContent,
                    adminName,
                    approvedAt,
                    signature.SignatureImage);

            // Save approval and signature snapshot in SQL Server.
            return _letterRequestRepository.ApproveRequest(
                letterRequestId,
                approvedByUserId,
                signature.SignatureImage,
                signature.ContentType,
                pdfReference);


        }

        public (string ApproverName, DateTime ApprovedAt, string SignatureBase64, string SignatureContentType)?
    GetApprovalSignature(int letterRequestId)
        {
            if (letterRequestId <= 0)
                return null;

            // Reuse the existing repository method.
            var approval =
                _letterRequestRepository.GetApprovedLetterForAdminDownload(letterRequestId);

            if (approval == null ||
                approval.SignatureImage == null ||
                approval.SignatureImage.Length == 0)
            {
                return null;
            }

            string contentType = "application/octet-stream";

            // Detect PNG.
            if (approval.SignatureImage.Length >= 8 &&
                approval.SignatureImage[0] == 0x89 &&
                approval.SignatureImage[1] == 0x50 &&
                approval.SignatureImage[2] == 0x4E &&
                approval.SignatureImage[3] == 0x47 &&
                approval.SignatureImage[4] == 0x0D &&
                approval.SignatureImage[5] == 0x0A &&
                approval.SignatureImage[6] == 0x1A &&
                approval.SignatureImage[7] == 0x0A)
            {
                contentType = "image/png";
            }
            // Detect JPG/JPEG.
            else if (approval.SignatureImage.Length >= 3 &&
                     approval.SignatureImage[0] == 0xFF &&
                     approval.SignatureImage[1] == 0xD8 &&
                     approval.SignatureImage[2] == 0xFF)
            {
                contentType = "image/jpeg";
            }

            return (
                approval.ApproverName,
                approval.ApprovedAt,
                Convert.ToBase64String(approval.SignatureImage),
                contentType
            );
        }
    }
}
