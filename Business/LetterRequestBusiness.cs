using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.LetterTemplates;
using Letter_Request_Management_System.Models.Dto;

namespace Letter_Request_Management_System.Business
{
    public class LetterRequestBusiness
    {
        private readonly LetterRequestRepository _letterRequestRepository;

        private readonly ServiceLetterTemplate _serviceLetterTemplate;
        private readonly EmploymentConfirmationLetterTemplate _employmentConfirmationLetterTemplate;
        private readonly SalaryConfirmationLetterTemplate _salaryConfirmationLetterTemplate;

        public LetterRequestBusiness(
            LetterRequestRepository letterRequestRepository,
            ServiceLetterTemplate serviceLetterTemplate,
            EmploymentConfirmationLetterTemplate employmentConfirmationLetterTemplate,
            SalaryConfirmationLetterTemplate salaryConfirmationLetterTemplate)
        {
            _letterRequestRepository = letterRequestRepository;
            _serviceLetterTemplate = serviceLetterTemplate;
            _employmentConfirmationLetterTemplate = employmentConfirmationLetterTemplate;
            _salaryConfirmationLetterTemplate = salaryConfirmationLetterTemplate;
        }

        // Create a new letter request
        public int CreateRequest(int userId, string letterTypeName)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(letterTypeName))
                return 0;

            // Retrieve employee information
            var employee = _letterRequestRepository.GetEmployeeForLetter(userId);

            if (employee == null)
                return 0;

            // Generate the appropriate letter
            string letterContent;

            switch (letterTypeName.Trim().ToLowerInvariant())
            {
                case "service letter":
                    letterContent = _serviceLetterTemplate.GenerateLetter(employee);
                    break;

                case "employment confirmation letter":
                    letterContent = _employmentConfirmationLetterTemplate.GenerateLetter(employee);
                    break;

                case "salary confirmation letter":

                    if (!employee.Salary.HasValue)
                        throw new InvalidOperationException(
                            "Salary information is not available for this employee.");

                    letterContent = _salaryConfirmationLetterTemplate.GenerateLetter(employee);
                    break;

                default:
                    throw new ArgumentException("Invalid letter type.");
            }

            // Save the generated letter content
            return _letterRequestRepository.CreateRequest(
                userId,
                letterTypeName.Trim(),
                letterContent);
        }

        public LetterRequestResponseDto? GetRequestById(int letterRequestId,int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
                return null;

            return _letterRequestRepository.GetRequestById(letterRequestId,userId);
        }

        public List<LetterRequestResponseDto> GetMyRequests(int userId)
        {
            if (userId <= 0)
                return new List<LetterRequestResponseDto>();

            return _letterRequestRepository.GetMyRequests(userId);
        }

        public string? GetApprovedPdfReference(int letterRequestId,int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
                return null;

            return _letterRequestRepository.GetApprovedPdfReference(
                letterRequestId,
                userId);
        }

        public ApprovedLetterDownloadDto? GetApprovedLetterForDownload(
    int letterRequestId,
    int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
                return null;

            return _letterRequestRepository.GetApprovedLetterForDownload(
                letterRequestId,
                userId);
        }

        public LetterRequestResponseDto? GetPendingLetterForPreview(int letterRequestId,int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
                return null;

            var request = _letterRequestRepository.GetRequestById(letterRequestId,userId);

            if (request == null)
                return null;

            if (!string.Equals(request.Status,"Pending",StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return request;
        }

        // Cancel a pending letter request (Employee)
        public int CancelRequest(int letterRequestId, int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
                return 0;

            return _letterRequestRepository.CancelRequest(
                letterRequestId,
                userId);
        }
    }
}
