using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.ResetPasswordService
{
    public class ResetPasswordService
    {
        private readonly IUserRepository _userRepository;

        public ResetPasswordService()
        {
            _userRepository = MarketMinds.App.UserRepository;
        }

        public ResetPasswordService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            try
            {
                return await _userRepository.ResetUserPasswordAsync(email, newPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }

        // Backward compatibility method for older code
        public bool ResetPassword(string email, string newPassword)
        {
            // Call the async method and wait for the result
            return ResetPasswordAsync(email, newPassword).GetAwaiter().GetResult();
        }
    }
}
