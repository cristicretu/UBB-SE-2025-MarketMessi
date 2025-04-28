using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace MarketMinds.Repositories
{
    public class UserRepository : IAccountRepository
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;
        private static readonly int ERROR_CODE = -1;

        public UserRepository(IConfiguration configuration)
        {
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
            Console.WriteLine($"User Repository initialized with base address: {httpClient.BaseAddress}");

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                var response = await httpClient.GetAsync($"account/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<User>(content, jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UserOrder>> GetUserOrdersAsync(int userId)
        {
            try
            {
                var response = await httpClient.GetAsync($"account/{userId}/orders");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<UserOrder>>(content, jsonOptions);
                }
                return new List<UserOrder>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user orders: {ex.Message}");
                return new List<UserOrder>();
            }
        }

        public async Task<double> GetBasketTotalAsync(int userId, int basketId)
        {
            try
            {
                var response = await httpClient.GetAsync($"account/{userId}/basket/{basketId}/total");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<double>(content, jsonOptions);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting basket total: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<Order>> CreateOrderFromBasketAsync(int userId, int basketId, double discountAmount = 0)
        {
            try
            {
                var orderRequest = new
                {
                    UserId = userId,
                    BasketId = basketId,
                    DiscountAmount = discountAmount
                };

                var response = await httpClient.PostAsJsonAsync($"account/orders", orderRequest);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Order>>(content, jsonOptions);
                }
                return new List<Order>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order from basket: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"account/{user.Id}", user);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        // Additional methods for user authentication that aren't part of IAccountRepository
        public async Task<bool> AuthenticateUserAsync(string username, string password)
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
                    return JsonSerializer.Deserialize<User>(content, jsonOptions);
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
                    return JsonSerializer.Deserialize<User>(content, jsonOptions);
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
                    return JsonSerializer.Deserialize<User>(content, jsonOptions);
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
                var existingUserByEmail = await GetUserByEmailAsync(user.Email);
                if (existingUserByEmail != null)
                {
                    return false;
                }

                // Check if username exists
                bool usernameTaken = await IsUsernameTakenAsync(user.Username);
                if (usernameTaken)
                {
                    return false;
                }

                // Verify we're sending all required fields
                var registerRequest = new
                {
                    Username = user.Username,
                    Email = user.Email,
                    Password = user.Password
                };
                var response = await httpClient.PostAsJsonAsync("users/register", registerRequest);
                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[UserRepository] Register API error response: {responseBody}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[UserRepository] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        private class UsernameCheckResult
        {
            public bool Exists { get; set; }
        }
    }
}