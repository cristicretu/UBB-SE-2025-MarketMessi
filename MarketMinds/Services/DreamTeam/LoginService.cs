using System;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.LoginService
{
    public class LoginService
    {
        private readonly IUserRepository _userRepository;

        public LoginService()
        {
            _userRepository = MarketMinds.App.UserRepository;
        }

        public LoginService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> AuthenticateUser(string username, string password)
        {
            return await _userRepository.ValidateCredentialsAsync(username, password);
        }

        public async Task<User> GetUserByCredentials(string username, string password)
        {
            try
            {
                if (await AuthenticateUser(username, password))
                {
                    return await _userRepository.GetUserByCredentialsAsync(username, password);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by credentials: {ex.Message}");
                return null;
            }
        }
    }
}