using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json; // Requires System.Net.Http.Json nuget package
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration; // For IConfiguration
using MarketMinds; // For App.CurrentUser
using MarketMinds.Services;

namespace Marketplace_SE.Services.DreamTeam // Consider moving to MarketMinds.Services namespace
{
    public class AccountPageService : IAccountPageService // Implementing the interface
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public AccountPageService(IConfiguration configuration/*, ILogger<AccountPageService> logger*/)
        {
            _httpClient = new HttpClient();

            _apiBaseUrl = configuration["ServerApiBaseUrl"] ?? "https://localhost:5000"; // Default fallback
            if (!_apiBaseUrl.EndsWith("/"))
            {
                _apiBaseUrl += "/";
            }
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        }

        // Renamed and made async to fetch from API
        public async Task<User> GetUserAsync(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            string requestUrl = $"{_apiBaseUrl}/api/account/{userId}";
            try
            {
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return null;
                    }
                    var user = await response.Content.ReadFromJsonAsync<User>();
                    return user;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                return null;
            }
            catch (JsonException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<User> GetCurrentLoggedInUserAsync()
        {
            int currentUserId = App.CurrentUser?.Id ?? 0;
            if (currentUserId > 0)
            {
                return await GetUserAsync(currentUserId);
            }
            return null;
        }

        public async Task<List<UserOrder>> GetUserOrdersAsync(int userId)
        {
            if (userId <= 0)
            {
                return new List<UserOrder>(); // Return empty list
            }

            string requestUrl = $"{_apiBaseUrl}/api/account/{userId}/orders";
            try
            {
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return new List<UserOrder>();
                    }
                    var orders = await response.Content.ReadFromJsonAsync<List<UserOrder>>();
                    return orders ?? new List<UserOrder>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new List<UserOrder>();
                }
            }
            catch (HttpRequestException ex)
            {
                return new List<UserOrder>();
            }
            catch (JsonException ex)
            {
                return new List<UserOrder>();
            }
            catch (Exception ex)
            {
                return new List<UserOrder>();
            }
        }
    }
}
