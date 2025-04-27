using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json; // Requires System.Net.Http.Json nuget package
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Diagnostics;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration; // For IConfiguration
using MarketMinds; // For App.CurrentUser
using MarketMinds.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Marketplace_SE.Services.DreamTeam // Consider moving to MarketMinds.Services namespace
{
    public class AccountPageService : IAccountPageService // Implementing the interface
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;
        private readonly JsonSerializerOptions jsonOptions;

        public AccountPageService(IConfiguration configuration)
        {
            httpClient = new HttpClient();

            // Get API base URL from configuration, same way as BasketService
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            Debug.WriteLine($"Using API base URL from configuration: {apiBaseUrl}");

            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }

            // Set base address to include api/account/ path
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/account/");

            // Set longer timeout for HTTP requests, same as BasketService
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Configure JSON options to match BasketService
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.Preserve
            };

            Debug.WriteLine($"AccountPageService initialized with API URL: {httpClient.BaseAddress}");
        }

        public async Task<User> GetUserAsync(int userId)
        {
            if (userId <= 0)
            {
                Debug.WriteLine($"Invalid user ID: {userId}");
                return null;
            }

            Debug.WriteLine($"Making API request to get user {userId}");

            try
            {
                // Use relative URL since BaseAddress already includes api/account/
                var response = await httpClient.GetAsync($"{userId}");
                Debug.WriteLine($"API response status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API response received for user {userId}");

                    try
                    {
                        var user = System.Text.Json.JsonSerializer.Deserialize<User>(responseContent, jsonOptions);

                        if (user != null)
                        {
                            Debug.WriteLine($"Successfully parsed user: ID={user.Id}, Username={user.Username}, Balance={user.Balance}");

                            // Ensure the user has a balance
                            if (user.Balance <= 0)
                            {
                                // Fallback value if API returns user with no balance
                                user.Balance = 100.0f;
                                Debug.WriteLine($"API returned user with no balance, set default: {user.Balance}");
                            }

                            return user;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to deserialize user - result was null");
                        }
                    }
                    catch (System.Text.Json.JsonException jsonException)
                    {
                        Debug.WriteLine($"JSON Deserialization error: {jsonException.Message}");

                        // Try manual parsing as fallback
                        if (TryParseUserFromJson(responseContent, out User user))
                        {
                            return user;
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API error: {response.StatusCode}, Content: {errorContent}");

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Debug.WriteLine("User not found");
                    }
                }
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine($"HTTP Request error: {exception.Message}");
                Debug.WriteLine("Server might be down or unreachable. Verify server is running.");

                // Provide diagnostic information
                if (exception.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {exception.InnerException.Message}");
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error retrieving user: {exception.GetType().Name}: {exception.Message}");
            }

            // If we got here, we failed to get the user from API
            if (App.CurrentUser?.Id == userId)
            {
                // Use App.CurrentUser as fallback only during development
                Debug.WriteLine("Using App.CurrentUser as fallback for development");
                return CreateUserCopy(App.CurrentUser);
            }

            return null;
        }

        public async Task<User> GetCurrentLoggedInUserAsync()
        {
            // Get user ID from App.CurrentUser
            int currentUserId = App.CurrentUser?.Id ?? 0;
            Debug.WriteLine($"Getting data for current user ID: {currentUserId}");

            if (currentUserId <= 0)
            {
                Debug.WriteLine("No current user ID available");
                return null;
            }

            // Get user data from API
            return await GetUserAsync(currentUserId);
        }

        public async Task<List<UserOrder>> GetUserOrdersAsync(int userId)
        {
            if (userId <= 0)
            {
                return new List<UserOrder>();
            }

            try
            {
                // Use relative URL since BaseAddress already includes api/account/
                var response = await httpClient.GetAsync($"{userId}/orders");
                Debug.WriteLine($"Orders API response status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var orders = System.Text.Json.JsonSerializer.Deserialize<List<UserOrder>>(responseContent, jsonOptions);
                    Debug.WriteLine($"Deserialized {orders?.Count ?? 0} orders");

                    // If we got orders, map the server model properties to client model
                    if (orders != null && orders.Any())
                    {
                        foreach (var order in orders)
                        {
                            // Map ItemName to Name if needed
                            if (!string.IsNullOrEmpty(order.ItemName) && string.IsNullOrEmpty(order.Name))
                            {
                                order.Name = order.ItemName;
                            }

                            // Map Price to Cost if needed
                            if (order.Price > 0 && order.Cost <= 0)
                            {
                                order.Cost = order.Price;
                            }

                            // Set a default order status if not provided
                            if (string.IsNullOrEmpty(order.OrderStatus))
                            {
                                order.OrderStatus = "Completed";
                            }

                            // Ensure each order has a Created timestamp
                            if (order.Created == 0)
                            {
                                order.Created = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            }
                        }

                        return orders;
                    }
                    else
                    {
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException exception)
            {
                if (exception.InnerException != null)
                {
                }
            }
            catch (Exception exception)
            {
            }

            // Return empty list if no orders found or error occurred
            return new List<UserOrder>();
        }

        // Helper method to get property value using reflection
        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    return (T)property.GetValue(obj);
                }
            }
            catch (Exception exception)
            {
            }
            return default(T);
        }

        // Helper to create a copy of a user object to avoid reference issues
        private User CreateUserCopy(User source)
        {
            if (source == null)
            {
                return null;
            }

            var copy = new User(source.Id, source.Username, source.Email, source.Token);
            copy.UserType = source.UserType;
            copy.Balance = source.Balance;
            copy.Rating = source.Rating;

            Debug.WriteLine($"Created user copy: ID={copy.Id}, Username={copy.Username}, Balance={copy.Balance}");
            return copy;
        }

        // Helper method to parse orders from JSON when normal deserialization fails
        private List<UserOrder> TryParseOrdersFromJson(string json)
        {
            try
            {
                var orders = new List<UserOrder>();
                var jsonDocument = JsonDocument.Parse(json);

                if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in jsonDocument.RootElement.EnumerateArray())
                    {
                        var order = new UserOrder();

                        if (element.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number)
                        {
                            order.Id = idProp.GetInt32();
                        }

                        // Try both name properties
                        if (element.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                        {
                            order.Name = nameProp.GetString();
                        }
                        else if (element.TryGetProperty("itemName", out var itemNameProp) && itemNameProp.ValueKind == JsonValueKind.String)
                        {
                            order.Name = itemNameProp.GetString();
                        }

                        if (element.TryGetProperty("description", out var descriptionProp) && descriptionProp.ValueKind == JsonValueKind.String)
                        {
                            order.Description = descriptionProp.GetString();
                        }

                        // Try both cost properties
                        if (element.TryGetProperty("cost", out var costProp) && costProp.ValueKind == JsonValueKind.Number)
                        {
                            order.Cost = costProp.GetSingle();
                        }
                        else if (element.TryGetProperty("price", out var priceProp) && priceProp.ValueKind == JsonValueKind.Number)
                        {
                            order.Cost = priceProp.GetSingle();
                        }

                        if (element.TryGetProperty("sellerId", out var sellerIdProp) && sellerIdProp.ValueKind == JsonValueKind.Number)
                        {
                            order.SellerId = sellerIdProp.GetInt32();
                        }

                        if (element.TryGetProperty("buyerId", out var buyerIdProp) && buyerIdProp.ValueKind == JsonValueKind.Number)
                        {
                            order.BuyerId = buyerIdProp.GetInt32();
                        }

                        if (element.TryGetProperty("created", out var createdProp))
                        {
                            if (createdProp.ValueKind == JsonValueKind.Number)
                            {
                                order.Created = (ulong)createdProp.GetInt64();
                            }
                        }

                        // Set default status
                        order.OrderStatus = "Completed";

                        orders.Add(order);
                    }
                }

                Debug.WriteLine($"Manually parsed {orders.Count} orders from JSON");
                return orders;
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Failed to manually parse orders: {exception.Message}");
                return new List<UserOrder>();
            }
        }

        // Try to parse a User from JSON response if automatic deserialization fails
        private bool TryParseUserFromJson(string json, out User user)
        {
            user = null;

            try
            {
                var jsonDocument = JsonDocument.Parse(json);
                var root = jsonDocument.RootElement;

                user = new User();

                if (root.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number)
                {
                    user.Id = idProp.GetInt32();
                }

                if (root.TryGetProperty("username", out var usernameProp) && usernameProp.ValueKind == JsonValueKind.String)
                {
                    user.Username = usernameProp.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String)
                {
                    user.Email = emailProp.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("balance", out var balanceProp))
                {
                    if (balanceProp.ValueKind == JsonValueKind.Number)
                    {
                        user.Balance = balanceProp.GetSingle();
                    }
                    else if (balanceProp.ValueKind == JsonValueKind.String &&
                            float.TryParse(balanceProp.GetString(), out float balanceValue))
                    {
                        user.Balance = balanceValue;
                    }
                }

                if (root.TryGetProperty("userType", out var userTypeProp) && userTypeProp.ValueKind == JsonValueKind.Number)
                {
                    user.UserType = userTypeProp.GetInt32();
                }

                if (root.TryGetProperty("rating", out var ratingProp) && ratingProp.ValueKind == JsonValueKind.Number)
                {
                    user.Rating = ratingProp.GetSingle();
                }

                // Ensure balance has a reasonable value
                if (user.Balance <= 0)
                {
                    user.Balance = 100.0f;
                }

                Debug.WriteLine($"Successfully manually parsed user: ID={user.Id}, Username={user.Username}, Balance={user.Balance}");
                return true;
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error parsing user from JSON: {exception.Message}");
                return false;
            }
        }
    }
}
