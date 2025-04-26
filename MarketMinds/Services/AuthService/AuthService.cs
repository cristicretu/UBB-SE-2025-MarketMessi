using System;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;

namespace MarketMinds.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                return await _userRepository.ValidateCredentialsAsync(username, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetUserByCredentialsAsync(string username, string password)
        {
            try
            {
                return await _userRepository.GetUserByCredentialsAsync(username, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by credentials: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            try
            {
                return await _userRepository.IsUsernameTakenAsync(username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if username is taken: {ex.Message}");
                return true; // Assume taken if error occurs
            }
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                int userId = await _userRepository.CreateUserAsync(user);
                return userId > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering user: {ex.Message}");
                return false;
            }
        }
    }
} 