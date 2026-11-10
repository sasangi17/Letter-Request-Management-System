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
            string? adminName =
                _letterRequestRepository.GetUserFullName(
                    approvedByUserId);

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
    }
}
