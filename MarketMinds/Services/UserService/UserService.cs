using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.AuthService;

namespace Marketplace_SE.Services
{
    public class UserService : IUserService
    {
        private const string ValidUserId = "12345";
        private readonly IAuthService _authService;

        public UserService()
        {
            _authService = MarketMinds.App.AuthService;
        }

        public UserService(IAuthService authService)
        {
            _authService = authService;
        }

        public string RetrieveUserId()
        {
            // Return the hardcoded user ID
            return ValidUserId;
        }

        public bool ValidateUserId(string enteredId)
        {
            // Compare the entered ID with the valid user ID
            return enteredId == ValidUserId;
        }

        // Delegate to AuthService for authentication operations
        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            return await _authService.AuthenticateUserAsync(username, password);
        }

        public async Task<User> GetUserByCredentialsAsync(string username, string password)
        {
            return await _authService.GetUserByCredentialsAsync(username, password);
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            return await _authService.IsUsernameTakenAsync(username);
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            return await _authService.RegisterUserAsync(user);
        }
    }
}
