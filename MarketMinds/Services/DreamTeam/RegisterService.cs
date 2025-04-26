using System;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.RegisterService
{
    public class RegisterService
    {
        private readonly IUserRepository _userRepository;

        public RegisterService()
        {
            _userRepository = MarketMinds.App.UserRepository;
        }

        public RegisterService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> IsUsernameTaken(string username)
        {
            try
            {
                return await _userRepository.IsUsernameTakenAsync(username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking username: {ex.Message}");
                return true; // Fail-safe: assume taken if error occurs
            }
        }

        public async Task<bool> RegisterUser(User user)
        {
            try
            {
                // Check if email exists
                var existingUserByEmail = await _userRepository.GetUserByEmailAsync(user.Email);
                if (existingUserByEmail != null)
                {
                    return false;
                }

                // Check if username exists
                if (await IsUsernameTaken(user.Username))
                {
                    return false;
                }

                // Create user and return success if Id > 0
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