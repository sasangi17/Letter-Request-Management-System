using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Dto;

namespace Letter_Request_Management_System.Business
{
    public class AdminBusiness
    {
        private readonly LetterRequestRepository _letterRequestRepository;

        public AdminBusiness(LetterRequestRepository letterRequestRepository)
        {
            _letterRequestRepository = letterRequestRepository;
        }

        public List<AdminLetterRequestDto> GetAllRequests()
        {
            return _letterRequestRepository.GetAllRequests();
        }

        public AdminLetterRequestDto? GetRequestById(int letterRequestId)
        {
            return _letterRequestRepository.GetAdminRequestById(letterRequestId);
        }

        public int ApproveRequest(
            int letterRequestId,
            int approvedByUserId,
            string signatureReference)
        {
            return _letterRequestRepository.ApproveRequest(
                letterRequestId,
                approvedByUserId,
                signatureReference);
        }
    }
}
