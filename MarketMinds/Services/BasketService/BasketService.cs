using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.BasketService
{
    public class BasketService : IBasketService
    {
        public const int MaxQuantityPerItem = 10;
        private const int NOUSER = 0;
        private const int NOITEM = 0;
        private const int NOBASKET = 0;
        private const int NODISCOUNT = 0;
        private const int NOQUANTITY = 0;
        private const int DEFAULTQUANTITY = 1;

        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;
        private readonly JsonSerializerOptions jsonOptions;

        // Constructor with configuration
        public BasketService(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/basket/");

            // Configure JSON options that match the server's configuration
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                // Changed from Preserve to IgnoreCycles
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            // Set longer timeout for HTTP requests
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public void AddProductToBasket(int userId, int productId, int quantity)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MaxQuantityPerItem);

                // Make the API call to add product to basket
                var response = httpClient.PostAsJsonAsync($"user/{userId}/product/{productId}", limitedQuantity).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to add product to basket: {ex.Message}", ex);
            }
        }

        public Basket GetBasketByUser(User user)
        {
            if (user == null || user.Id <= NOUSER)
            {
                throw new ArgumentException("Valid user must be provided");
            }

            try
            {
                string fullUrl = $"{apiBaseUrl}api/basket/user/{user.Id}";

                var response = httpClient.GetAsync(fullUrl).Result;
                response.EnsureSuccessStatusCode();

                // Use the custom JSON converter to properly deserialize the response
                var responseContent = response.Content.ReadAsStringAsync().Result;

                try
                {
                    // Create improved JsonSerializerOptions optimized for the client
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        // Use IgnoreCycles instead of Preserve
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    };

                    // Add UserJsonConverter to handle password field type mismatch
                    options.Converters.Add(new MarketMinds.Services.UserJsonConverter());

                    // Deserialize directly
                    var basket = JsonSerializer.Deserialize<Basket>(responseContent, options);

                    // Make sure we have an Items collection
                    if (basket.Items == null)
                    {
                        basket.Items = new List<BasketItem>();
                    }

                    return basket;
                }
                catch (JsonException ex)
                {
                    // Try to fallback to a simpler deserialization
                    var fallbackBasket = new Basket { Id = user.Id, Items = new List<BasketItem>() };
                    return fallbackBasket;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to retrieve user's basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve user's basket", ex);
            }
        }

        public void RemoveProductFromBasket(int userId, int productId)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Make the API call to remove product from basket
                var response = httpClient.DeleteAsync($"user/{userId}/product/{productId}").Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to remove product from basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to remove product from basket: {ex.Message}", ex);
            }
        }

        public void UpdateProductQuantity(int userId, int productId, int quantity)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }
            if (quantity < NOQUANTITY)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MaxQuantityPerItem);

                // Make the API call to update product quantity
                var response = httpClient.PutAsJsonAsync($"user/{userId}/product/{productId}", limitedQuantity).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to update product quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to update product quantity: {ex.Message}", ex);
            }
        }

        public bool ValidateQuantityInput(string quantityText, out int quantity)
        {
            // Initialize output parameter
            quantity = NOQUANTITY;

            // Check if the input is empty
            if (string.IsNullOrWhiteSpace(quantityText))
            {
                return false;
            }

            // Try to parse the input string to an integer
            if (!int.TryParse(quantityText, out quantity))
            {
                return false;
            }

            // Check if quantity is non-negative
            if (quantity < NOQUANTITY)
            {
                return false;
            }

            return true;
        }

        public int GetLimitedQuantity(int quantity)
        {
            return Math.Min(quantity, MaxQuantityPerItem);
        }

        public void ClearBasket(int userId)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }

            try
            {
                // Clear the basket through API
                var response = httpClient.DeleteAsync($"user/{userId}/clear").Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not clear basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not clear basket: {ex.Message}", ex);
            }
        }

        public bool ValidateBasketBeforeCheckOut(int basketId)
        {
            if (basketId <= NOBASKET)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                // Validate basket through API
                var response = httpClient.GetAsync($"{basketId}/validate").Result;
                response.EnsureSuccessStatusCode();

                var responseContent = response.Content.ReadAsStringAsync().Result;

                try
                {
                    return JsonSerializer.Deserialize<bool>(responseContent, jsonOptions);
                }
                catch (JsonException)
                {
                    // Try a simple parsing approach as fallback
                    responseContent = responseContent.Trim().ToLower();
                    if (responseContent == "true")
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not validate basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not validate basket: {ex.Message}", ex);
            }
        }

        public void ApplyPromoCode(int basketId, string code)
        {
            if (basketId <= NOBASKET)
            {
                throw new ArgumentException("Invalid basket ID");
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Promo code cannot be empty");
            }

            try
            {
                // Apply promo code through API
                var response = httpClient.PostAsJsonAsync($"{basketId}/promocode", code).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Invalid promo code");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not apply promo code: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Class for deserializing discount rate response
        private class DiscountResponse
        {
            public double DiscountRate { get; set; }
        }

        // Add a new method to get the discount for a promo code
        public double GetPromoCodeDiscount(string code, double subtotal)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return NODISCOUNT;
            }

            try
            {
                string normalizedCode = code.Trim().ToUpper();
                var response = httpClient.PostAsJsonAsync($"1/promocode", normalizedCode).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var result = JsonSerializer.Deserialize<DiscountResponse>(responseContent, jsonOptions);
                    double discount = subtotal * result.DiscountRate;
                    return discount;
                }

                return NODISCOUNT;
            }
            catch (Exception ex)
            {
                return NODISCOUNT;
            }
        }

        // Add a new method to calculate basket totals
        public BasketTotals CalculateBasketTotals(int basketId, string promoCode)
        {
            if (basketId <= NOBASKET)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                // Get totals from API
                string endpoint = $"{basketId}/totals";
                if (!string.IsNullOrEmpty(promoCode))
                {
                    endpoint += $"?promoCode={promoCode}";
                }

                var response = httpClient.GetAsync(endpoint).Result;
                response.EnsureSuccessStatusCode();

                var responseContent = response.Content.ReadAsStringAsync().Result;

                try
                {
                    var totals = JsonSerializer.Deserialize<BasketTotals>(responseContent, jsonOptions);
                    return totals;
                }
                catch (JsonException)
                {
                    // Create a default totals object
                    return new BasketTotals();
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not calculate basket totals: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not calculate basket totals: {ex.Message}", ex);
            }
        }

        public void DecreaseProductQuantity(int userId, int productId)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Decrease quantity through API
                var response = httpClient.PutAsync($"user/{userId}/product/{productId}/decrease", null).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not decrease quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not decrease quantity: {ex.Message}", ex);
            }
        }

        public void IncreaseProductQuantity(int userId, int productId)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= NOITEM)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Increase quantity through API
                var response = httpClient.PutAsync($"user/{userId}/product/{productId}/increase", null).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not increase quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not increase quantity: {ex.Message}", ex);
            }
        }

        public async Task<bool> CheckoutBasketAsync(int userId, int basketId, double discountAmount = 0, double totalAmount = 0)
        {
            if (userId <= NOUSER)
            {
                throw new ArgumentException("Invalid user ID");
            }

            if (basketId <= NOBASKET)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                // First, validate the basket before checkout
                if (!ValidateBasketBeforeCheckOut(basketId))
                {
                    throw new InvalidOperationException("Basket validation failed");
                }

                // Get the current basket totals
                var basketTotals = CalculateBasketTotals(basketId, null);
                System.Diagnostics.Debug.WriteLine($"DIAGNOSTIC: CheckoutBasketAsync - Raw basketTotal: {basketTotals.TotalAmount}, Provided discount: {discountAmount}, Provided total: {totalAmount}");

                // Use provided values if they are valid
                if (discountAmount > 0 && totalAmount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"DIAGNOSTIC: Using provided discount ({discountAmount}) and total ({totalAmount})");
                    basketTotals.Discount = discountAmount;
                    basketTotals.TotalAmount = totalAmount;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"DIAGNOSTIC: No valid discount/total provided, using calculated totals");
                }

                // Create an HTTP client for account API
                using var httpClientAccount = new HttpClient();
                httpClientAccount.BaseAddress = new Uri(apiBaseUrl);
                httpClientAccount.Timeout = TimeSpan.FromSeconds(30);

                // Create request for the account API to create orders from the basket
                // Include discount information
                var request = new
                {
                    BasketId = basketId,
                    DiscountAmount = basketTotals.Discount,
                    TotalAmount = basketTotals.TotalAmount
                };

                System.Diagnostics.Debug.WriteLine($"Checkout request: BasketId={basketId}, Discount={basketTotals.Discount}, Total={basketTotals.TotalAmount}");

                // Call the account API to create the order
                var response = await httpClientAccount.PostAsJsonAsync($"api/account/{userId}/orders", request);

                // Handle the response
                if (response.IsSuccessStatusCode)
                {
                    // If successful order creation, the API should have cleared the basket already
                    System.Diagnostics.Debug.WriteLine($"Successfully created order from basket {basketId} for user {userId}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error creating order: {response.StatusCode}, Content: {errorContent}");

                    // If it's a balance issue (400 status code), throw a more user-friendly message
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest &&
                        errorContent.Contains("Insufficient funds"))
                    {
                        throw new InvalidOperationException(errorContent);
                    }

                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not checkout basket: {ex.Message}", ex);
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException as-is to preserve the message
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not checkout basket: {ex.Message}", ex);
            }
        }
    }
}