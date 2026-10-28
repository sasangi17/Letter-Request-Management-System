using Letter_Request_Management_System.Data.Repository;

namespace Letter_Request_Management_System.Business
{
    public class AdminSignatureBusiness
    {
        private readonly AdminSignatureRepository _repository;

        public AdminSignatureBusiness(AdminSignatureRepository repository)
        {
            _repository = repository;
        }

        public bool SaveSignatureReference(int userId,string signatureReference)
        {
            if (userId <= 0)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(signatureReference))
            {
                return false;
            }

            return _repository.SaveOrUpdateSignatureReference(userId,signatureReference);
        }

        public string? GetSignatureReference(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            return _repository.GetSignatureReference(userId);
        }
    }
}
