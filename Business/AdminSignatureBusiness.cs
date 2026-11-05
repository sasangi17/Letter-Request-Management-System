using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Domain;

namespace Letter_Request_Management_System.Business
{
    public class AdminSignatureBusiness
    {
        private readonly AdminSignatureRepository _repository;

        public AdminSignatureBusiness(AdminSignatureRepository repository)
        {
            _repository = repository;
        }

        public bool SaveSignature(
            int adminUserId,
            byte[] signatureImage,
            string contentType,
            string? fileName)
        {
            if (adminUserId <= 0 ||
                signatureImage == null ||
                signatureImage.Length == 0 ||
                string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }

            return _repository.SaveOrUpdateSignature(
                adminUserId,
                signatureImage,
                contentType,
                fileName);
        }

        public AdminSignatureMl? GetSignature(int adminUserId)
        {
            if (adminUserId <= 0)
                return null;

            return _repository.GetSignature(adminUserId);
        }
    }
}