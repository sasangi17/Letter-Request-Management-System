using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Dto;

namespace Letter_Request_Management_System.Business
{
    public class LetterRequestBusiness
    {
        private readonly LetterRequestRepository _letterRequestRepository;

        public LetterRequestBusiness(LetterRequestRepository letterRequestRepository)
        {
            _letterRequestRepository = letterRequestRepository;
        }

        public int CreateRequest(int userId, int letterTypeId)
        {
            if (userId <= 0 || letterTypeId <= 0)
                return 0;

            return _letterRequestRepository.CreateRequest(userId,letterTypeId);
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

            return _letterRequestRepository.GetApprovedPdfReference(letterRequestId,userId);
        }

        public (
            string LetterContent,
            string SignatureReference,
            string ApproverName,
            DateTime ApprovedAt
        )? GetApprovedLetterForDownload(int letterRequestId, int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
                return null;

            return _letterRequestRepository.GetApprovedLetterForDownload(letterRequestId,userId);
        }


        public LetterRequestResponseDto? GetPendingLetterForPreview(
            int letterRequestId,
            int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
                return null;

            // Get the request belonging to this employee.
            var request = _letterRequestRepository.GetRequestById(
                letterRequestId,
                userId);

            if (request == null)
                return null;

            // Only pending letters can be previewed.
            if (!string.Equals(
                request.Status,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return request;
        }

    }
}
