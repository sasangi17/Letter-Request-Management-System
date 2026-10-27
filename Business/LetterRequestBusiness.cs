using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Dto;

namespace Letter_Request_Management_System.Business
{
    public class LetterRequestBusiness
    {
        private readonly LetterRequestRepository _repository;

        public LetterRequestBusiness(
            LetterRequestRepository repository)
        {
            _repository = repository;
        }

        public int CreateRequest(int userId, int letterTypeId)
        {
            if (userId <= 0 || letterTypeId <= 0)
            {
                return 0;
            }

            return _repository.CreateRequest(userId, letterTypeId);
        }

        public LetterRequestResponseDto? GetRequestById(
            int letterRequestId, int userId)
        {
            if (letterRequestId <= 0 || userId <= 0)
            {
                return null;
            }

            return _repository.GetRequestById(
                letterRequestId, userId);
        }

        public List<LetterRequestResponseDto> GetMyRequests(int userId)
        {
            if (userId <= 0)
            {
                return new List<LetterRequestResponseDto>();
            }

            return _repository.GetMyRequests(userId);
        }
    }
}