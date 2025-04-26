using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Models;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserRepository(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _baseUrl = configuration["ApiSettings:BaseUrl"];
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                var loginRequest = new
                {
                    Username = username,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/users/login", loginRequest);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating credentials: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetUserByCredentialsAsync(string username, string password)
        {
            try
            {
                var loginRequest = new
                {
                    Username = username,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/users/login", loginRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userDto = JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);
                    return userDto?.ToDomainUser();
                }
                
                return null;
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
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/users/{username}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userDto = JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);
                    return userDto?.ToDomainUser();
                }
                
                return null;
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
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/users/by-email/{email}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userDto = JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);
                    return userDto?.ToDomainUser();
                }
                
                return null;
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
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/users/check-username/{username}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<UsernameCheckResult>(content, _jsonOptions);
                    return result.Exists;
                }
                
                // Default to true (username taken) if there's an error
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking username: {ex.Message}");
                return true;
            }
        }

        public async Task<int> CreateUserAsync(User newUser)
        {
            try
            {
                var userDto = new UserDto(newUser);
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/users/register", userDto);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var createdUserDto = JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);
                    return createdUserDto?.Id ?? -1;
                }
                
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(string email, string newPassword)
        {
            try
            {
                var resetRequest = new
                {
                    Email = email,
                    NewPassword = newPassword
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/users/reset-password", resetRequest);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }
        
        private class UsernameCheckResult
        {
            public bool Exists { get; set; }
        }
    }
}
