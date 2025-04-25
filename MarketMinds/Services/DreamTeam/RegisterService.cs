using System;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;

namespace MarketMinds.Services.RegisterService
{
    public class RegisterService
    {
        private readonly UserRepository userRepository;

        public RegisterService()
        {
            userRepository = new UserRepository();
        }

        public async Task<bool> IsUsernameTaken(string username)
        {
            try
            {
                var user = await Task.Run(() => userRepository.GetUserByUsername(username));
                return user != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking username: {ex.Message}");
                return true; // Fail-safe: assume taken if error occurs
            }
        }

        public async Task<bool> RegisterUser(User user)
        {
            // Check if email exists
            if (await Task.Run(() => userRepository.GetUserByEmail(user.Email)) != null)
            {
                return false;
            }
            // Check if username exists
            if (await IsUsernameTaken(user.Username))
            {
                return false;
            }

            user.Id = userRepository.CreateUser(user);
            return true;
        }
    }
}