using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Models;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.UserService
{
    public class UserService : IUserService
    {
        private static readonly int MINIMUM_USER_ID = 0;
        private static readonly int ERROR_CODE = -1;
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public UserService(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            }

            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }

            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }

            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
            Console.WriteLine($"Initialized HTTP client with base address: {httpClient.BaseAddress}");

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                return await ValidateCredentialsAsync(username, password);
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
                var loginRequest = new
                {
                    Username = username,
                    Password = password
                };

                var response = await httpClient.PostAsJsonAsync("users/login", loginRequest);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userDto = JsonSerializer.Deserialize<UserDto>(content, jsonOptions);
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
                var response = await httpClient.GetAsync($"users/{username}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userDto = JsonSerializer.Deserialize<UserDto>(content, jsonOptions);
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
                var response = await httpClient.GetAsync($"users/by-email/{email}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userDto = JsonSerializer.Deserialize<UserDto>(content, jsonOptions);
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
                var response = await httpClient.GetAsync($"users/check-username/{username}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<UsernameCheckResult>(content, jsonOptions);
                    return result.Exists;
                }
                // Default to true (username taken) if there's an error
                return true;
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
                // Check if email exists
                var existingUserByEmail = await GetUserByEmailAsync(user.Email);
                if (existingUserByEmail != null)
                {
                    return false;
                }

                // Check if username exists
                if (await IsUsernameTakenAsync(user.Username))
                {
                    return false;
                }

                // Create user and return success if Id > 0
                int userId = await CreateUserAsync(user);
                return userId > MINIMUM_USER_ID;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering user: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                var loginRequest = new
                {
                    Username = username,
                    Password = password
                };

                var response = await httpClient.PostAsJsonAsync("users/login", loginRequest);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating credentials: {ex.Message}");
                return false;
            }
        }

        private async Task<int> CreateUserAsync(User newUser)
        {
            try
            {
                var userDto = new UserDto(newUser);
                var response = await httpClient.PostAsJsonAsync("users/register", userDto);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var createdUserDto = JsonSerializer.Deserialize<UserDto>(content, jsonOptions);
                    return createdUserDto?.Id ?? ERROR_CODE;
                }
                return ERROR_CODE;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return ERROR_CODE;
            }
        }
        private class UsernameCheckResult
        {
            public bool Exists { get; set; }
        }
    }
}