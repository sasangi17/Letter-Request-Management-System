using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Dto;
using Letter_Request_Management_System.Services;
using Microsoft.AspNetCore.Hosting;

namespace Letter_Request_Management_System.Business
{
    public class AdminBusiness
    {
        private readonly LetterRequestRepository _letterRequestRepository;
        private readonly AdminSignatureBusiness _adminSignatureBusiness;
        private readonly AdminSignatureFileService _signatureFileService;
        private readonly SignedLetterPdfService _signedLetterPdfService;
        private readonly IWebHostEnvironment _environment;

        public AdminBusiness(
            LetterRequestRepository letterRequestRepository,
            AdminSignatureBusiness adminSignatureBusiness,
            AdminSignatureFileService signatureFileService,
            SignedLetterPdfService signedLetterPdfService,
            IWebHostEnvironment environment)
        {
            _letterRequestRepository = letterRequestRepository;
            _adminSignatureBusiness = adminSignatureBusiness;
            _signatureFileService = signatureFileService;
            _signedLetterPdfService = signedLetterPdfService;
            _environment = environment;
        }

        public List<AdminLetterRequestDto> GetAllRequests()
        {
            return _letterRequestRepository.GetAllRequests();
        }

        public AdminLetterRequestDto? GetRequestById(int letterRequestId)
        {
            return _letterRequestRepository.GetAdminRequestById(letterRequestId);
        }

        public int ApproveRequest(int letterRequestId,int approvedByUserId)
        {
            if (letterRequestId <= 0 || approvedByUserId <= 0)
                return 0;

            var request = _letterRequestRepository.GetAdminRequestById(letterRequestId);

            if (request == null)
                return 0;

            if (!string.Equals(request.Status,"Pending",StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            // Get the signature saved for this administrator.
            string? signatureReference =_adminSignatureBusiness.GetSignatureReference(approvedByUserId);

            if (string.IsNullOrWhiteSpace(signatureReference))
                return 3;

            // Resolve the saved signature image to its physical path.
            string? signatureImagePath = _signatureFileService.GetSignatureFullPath(signatureReference);

            if (string.IsNullOrWhiteSpace(signatureImagePath) ||
                !File.Exists(signatureImagePath))
            {
                return 4;
            }

            string? adminName =_letterRequestRepository.GetUserFullName(approvedByUserId);

            if (string.IsNullOrWhiteSpace(adminName))
                return 5;

            DateTime approvedAt = DateTime.Now;

            // Generate the signed PDF before saving its reference.
            string pdfReference =
                _signedLetterPdfService.GenerateSignedPdf(
                    request.LetterContent,
                    adminName,
                    approvedAt,
                    signatureImagePath);

            try
            {
                int result =
                    _letterRequestRepository.ApproveRequest(
                        letterRequestId,
                        approvedByUserId,
                        signatureReference,
                        pdfReference);

                if (result != 1)
                    DeleteGeneratedPdf(pdfReference);

                return result;
            }
            catch
            {
                DeleteGeneratedPdf(pdfReference);
                throw;
            }
        }

        private void DeleteGeneratedPdf(string pdfReference)
        {
            try
            {
                string fileName = Path.GetFileName(pdfReference);

                if (string.IsNullOrWhiteSpace(fileName) ||
                    !string.Equals(
                        Path.GetExtension(fileName),
                        ".pdf",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                string pdfPath = Path.Combine(
                    _environment.ContentRootPath,
                    "App_Data",
                    "SignedLetters",
                    fileName);

                if (File.Exists(pdfPath))
                {
                    File.Delete(pdfPath);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to delete generated PDF '{pdfReference}': {ex.Message}");
            }
        }
    }
}
