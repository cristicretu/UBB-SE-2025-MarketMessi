using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Test.Services.UserServiceTest
{
    public class UserRepositoryMock : IUserRepository
    {
        private readonly List<User> _users = new();
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public Task<bool> UsernameExistsAsync(string username)
        {
            bool exists = _users.Exists(u => u.Username == username);
            return Task.FromResult(exists);
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            bool exists = _users.Exists(u => u.Email == email);
            return Task.FromResult(exists);
        }

        public Task<User> RegisterUserAsync(string username, string email, string passwordHash)
        {
            var newUser = new User
            {
                Id = _users.Count + 1,
                Username = username,
                Email = email,
                Password = passwordHash
            };

            _users.Add(newUser);
            return Task.FromResult(newUser);
        }

        public Task<User> FindUserByUsernameAsync(string username)
        {
            var user = _users.Find(u => u.Username == username);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return Task.FromResult(user);
        }

        // Helper methods for tests
        public void AddTestUser(User user) => _users.Add(user);

        // Optionally keep the original Raw methods for backward compatibility
        public Task<string> AuthenticateUserRawAsync(string username, string password)
        {
            var user = _users.Find(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                throw new Exception("Authentication failed");
            }
            return Task.FromResult(JsonSerializer.Serialize(user, _jsonOptions));
        }

        public Task<string> GetUserByUsernameRawAsync(string username)
        {
            var user = _users.Find(u => u.Username == username);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return Task.FromResult(JsonSerializer.Serialize(user, _jsonOptions));
        }

        public Task<string> GetUserByEmailRawAsync(string email)
        {
            var user = _users.Find(u => u.Email == email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return Task.FromResult(JsonSerializer.Serialize(user, _jsonOptions));
        }

        public Task<string> CheckUsernameRawAsync(string username)
        {
            bool exists = _users.Exists(u => u.Username == username);
            var result = new UsernameCheckResult { Exists = exists };
            return Task.FromResult(JsonSerializer.Serialize(result, _jsonOptions));
        }

        public Task<string> RegisterUserRawAsync(object registerRequest)
        {
            // Extract properties via reflection since we're given an anonymous object
            var properties = registerRequest.GetType().GetProperties();
            string username = properties.First(p => p.Name == "Username").GetValue(registerRequest)?.ToString();
            string email = properties.First(p => p.Name == "Email").GetValue(registerRequest)?.ToString();
            string password = properties.First(p => p.Name == "Password").GetValue(registerRequest)?.ToString();

            var newUser = new User
            {
                Id = _users.Count + 1,
                Username = username,
                Email = email,
                Password = password
            };
            _users.Add(newUser);
            return Task.FromResult(JsonSerializer.Serialize(newUser, _jsonOptions));
        }

        public Task<string> GetUserByIdRawAsync(int userId)
        {
            var user = _users.Find(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return Task.FromResult(JsonSerializer.Serialize(user, _jsonOptions));
        }
    }

    public class UsernameCheckResult
    {
        public bool Exists { get; set; }
    }
}