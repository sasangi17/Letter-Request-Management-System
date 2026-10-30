using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Dto;
using Letter_Request_Management_System.Services;

namespace Letter_Request_Management_System.Business
{
    public class AdminBusiness
    {
        private readonly LetterRequestRepository _letterRequestRepository;
        private readonly AdminSignatureBusiness _adminSignatureBusiness;
        private readonly AdminSignatureFileService _signatureFileService;

        public AdminBusiness(
            LetterRequestRepository letterRequestRepository,
            AdminSignatureBusiness adminSignatureBusiness,
            AdminSignatureFileService signatureFileService)
        {
            _letterRequestRepository = letterRequestRepository;
            _adminSignatureBusiness = adminSignatureBusiness;
            _signatureFileService = signatureFileService;
        }

        public List<AdminLetterRequestDto> GetAllRequests()
        {
            return _letterRequestRepository.GetAllRequests();
        }

        public AdminLetterRequestDto? GetRequestById(int letterRequestId)
        {
            return _letterRequestRepository.GetAdminRequestById(
                letterRequestId);
        }

        public int ApproveRequest(
            int letterRequestId,
            int approvedByUserId)
        {
            if (letterRequestId <= 0 || approvedByUserId <= 0)
                return 0;

            // Get the requested letter.
            var request =
                _letterRequestRepository.GetAdminRequestById(
                    letterRequestId);

            if (request == null)
                return 0;

            // Only pending requests can be approved.
            if (!string.Equals(
                request.Status,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            // Get the administrator's saved signature reference.
            string? signatureReference =
                _adminSignatureBusiness.GetSignatureReference(
                    approvedByUserId);

            if (string.IsNullOrWhiteSpace(signatureReference))
                return 3;

            // Verify that the signature image exists.
            string? signatureImagePath = _signatureFileService.GetSignatureFullPath(signatureReference);

            if (string.IsNullOrWhiteSpace(signatureImagePath) ||
                !File.Exists(signatureImagePath))
            {
                return 4;
            }

            // Verify that the administrator exists.
            string? adminName =
                _letterRequestRepository.GetUserFullName(
                    approvedByUserId);

            if (string.IsNullOrWhiteSpace(adminName))
                return 5;

           
            return _letterRequestRepository.ApproveRequest(
                letterRequestId,
                approvedByUserId,
                signatureReference,
                string.Empty);
        }
    }
}
