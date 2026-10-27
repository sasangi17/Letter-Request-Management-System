using Letter_Request_Management_System.Data.Repository;
using Letter_Request_Management_System.Models.Domain;


namespace Letter_Request_Management_System.Business
{
    public class AuthBusiness
    {
        private readonly AuthRepository _authRepository;

        public AuthBusiness(AuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public UserMl? Login(string email, string password)
        {
            return _authRepository.Login(email, password);
        }
    }
}
