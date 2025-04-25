using System;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace Marketplace_SE
{
    public class RegisterViewModel
    {
        public RegisterViewModel()
        {
            // Constructor
        }

        public Task<bool> CreateNewUser(User user)
        {
            // In a real app, this would save to a database
            Console.WriteLine($"Created user: {user.Username}, {user.Email}");
            return Task.FromResult(true);
        }

        public Task<bool> IsUsernameTaken(string username)
        {
            // Mock implementation
            return Task.FromResult(false);
        }
    }

    // Provide an alias for backward compatibility
    public class SignUpViewModel : RegisterViewModel
    {
        // Inherits functionality from RegisterViewModel
    }
}