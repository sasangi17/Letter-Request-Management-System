using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Domain;

namespace Letter_Request_Management_System.Business
{
    public class LetterTypeBusiness
    {
        private readonly LetterTypeRepository _letterTypeRepository;

        public LetterTypeBusiness(
            LetterTypeRepository letterTypeRepository)
        {
            _letterTypeRepository = letterTypeRepository;
        }

        public List<LetterTypeMl> GetAllLetterTypes()
        {
            return _letterTypeRepository.GetAllLetterTypes();
        }
    }
}