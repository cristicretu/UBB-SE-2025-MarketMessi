using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.ServiceProxy;
using MarketMinds.Shared.IRepository;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly UserServiceProxy repository;

        public UserService(IConfiguration configuration)
        {
            repository = new UserServiceProxy(configuration);
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                return await repository.AuthenticateUserAsync(username, password);
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
                var sharedUser = await repository.GetUserByCredentialsAsync(username, password);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by credentials: {ex.Message}");
                return null;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                var sharedUser = await repository.GetUserByUsernameAsync(username);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by username: {ex.Message}");
                return null;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                var sharedUser = await repository.GetUserByEmailAsync(email);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by email: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            try
            {
                return await repository.IsUsernameTakenAsync(username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if username is taken: {ex.Message}");
                return true;
            }
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                if (user == null)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(user.Username))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(user.Email))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(user.Password))
                {
                    return false;
                }
                var sharedUser = ConvertToSharedUser(user);
                bool result = await repository.RegisterUserAsync(sharedUser);
                return result;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[UserService] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                var sharedUser = await repository.GetUserByIdAsync(userId);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by ID: {ex.Message}");
                return null;
            }
        }

        // Helper methods to convert between domain and shared models
        private User ConvertToDomainUser(MarketMinds.Shared.Models.User sharedUser)
        {
            if (sharedUser == null)
            {
                return null;
            }

            return new User
            {
                Id = sharedUser.Id,
                Username = sharedUser.Username,
                Email = sharedUser.Email,
                PasswordHash = sharedUser.PasswordHash,
                Password = sharedUser.Password,
                UserType = sharedUser.UserType,
                Balance = sharedUser.Balance,
                Rating = sharedUser.Rating
            };
        }

        private MarketMinds.Shared.Models.User ConvertToSharedUser(User domainUser)
        {
            if (domainUser == null)
            {
                return null;
            }
            return new MarketMinds.Shared.Models.User
            {
                Id = domainUser.Id,
                Username = domainUser.Username,
                Email = domainUser.Email,
                PasswordHash = domainUser.PasswordHash,
                Password = domainUser.Password,
                UserType = domainUser.UserType,
                Balance = domainUser.Balance,
                Rating = domainUser.Rating
            };
        }
    }
}