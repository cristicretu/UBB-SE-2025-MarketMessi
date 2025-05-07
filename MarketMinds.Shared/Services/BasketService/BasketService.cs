using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.IRepository;
using System.Text.Json.Nodes;

namespace MarketMinds.Shared.Services.BasketService
{
    public class BasketService : IBasketService
    {
        public const int MAXIMUM_QUANTITY_PER_ITEM = 10;
        private const int MINIMUM_USER_ID = 0;
        private const int MINIMUM_ITEM_ID = 0;
        private const int MINIMUM_BASKET_ID = 0;
        private const double MINIMUM_DISCOUNT = 0;
        private const int MINIMUM_QUANTITY = 0;
        private const int DEFAULT_QUANTITY = 1;
        private const int INVALID_USER_ID = -1;
        private const int INVALID_BASKET_ID = -1;

        private readonly IBasketRepository basketRepository;
        private readonly JsonSerializerOptions jsonOptions;

        // Constructor with configuration
        public BasketService(IBasketRepository basketRepository)
        {
            this.basketRepository = basketRepository;

            // Configure JSON options
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }

        public void AddProductToBasket(int userId, int productId, int quantity)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MAXIMUM_QUANTITY_PER_ITEM);

                // Make the API call to add product to basket
                basketRepository.AddProductToBasketRaw(userId, productId, limitedQuantity);
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to add product to basket: {ex.Message}", ex);
            }
        }

        public Basket GetBasketByUser(User user)
        {
            if (user == null || user.Id <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Valid user must be provided");
            }

            try
            {
                // Use the proxy repository to get the raw JSON response
                var responseJson = basketRepository.GetBasketByUserRaw(user.Id);

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
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    };

                    // Add UserJsonConverter to handle password field type mismatch
                    options.Converters.Add(new MarketMinds.Shared.Services.UserJsonConverter());

                    // Deserialize directly
                    var basket = JsonSerializer.Deserialize<Basket>(responseJson, options);

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
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Make the API call to remove product from basket
                basketRepository.RemoveProductFromBasketRaw(userId, productId);
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
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }
            if (quantity < MINIMUM_QUANTITY)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MAXIMUM_QUANTITY_PER_ITEM);

                // Make the API call to update product quantity
                basketRepository.UpdateProductQuantityRaw(userId, productId, limitedQuantity);
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
            quantity = MINIMUM_QUANTITY;

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
            if (quantity < MINIMUM_QUANTITY)
            {
                return false;
            }

            return true;
        }

        public int GetLimitedQuantity(int quantity)
        {
            return Math.Min(quantity, MAXIMUM_QUANTITY_PER_ITEM);
        }

        public void ClearBasket(int userId)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }

            try
            {
                // Clear the basket through API
                basketRepository.ClearBasketRaw(userId);
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
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                // Validate basket through API
                var responseContent = basketRepository.ValidateBasketBeforeCheckOutRaw(basketId);

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
            if (basketId <= MINIMUM_BASKET_ID)
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
                var response = basketRepository.ApplyPromoCodeRaw(basketId, code);

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Invalid promo code");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not apply promo code: {ex.Message}", ex);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not apply promo code: {ex.Message}", ex);
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
                return MINIMUM_DISCOUNT;
            }

            try
            {
                var responseJson = basketRepository.GetPromoCodeDiscountRaw(code);

                try
                {
                    var result = JsonSerializer.Deserialize<DiscountResponse>(responseJson, jsonOptions);
                    double discount = subtotal * result.DiscountRate;
                    return discount;
                }
                catch (JsonException)
                {
                    return MINIMUM_DISCOUNT;
                }
            }
            catch (Exception ex)
            {
                return MINIMUM_DISCOUNT;
            }
        }

        // Add a new method to calculate basket totals
        public BasketTotals CalculateBasketTotals(int basketId, string promoCode)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                // Get totals from API
                var responseJson = basketRepository.CalculateBasketTotalsRaw(basketId, promoCode);

                try
                {
                    var totals = JsonSerializer.Deserialize<BasketTotals>(responseJson, jsonOptions);
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
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Decrease quantity through API
                basketRepository.DecreaseProductQuantityRaw(userId, productId);
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
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Increase quantity through API
                basketRepository.IncreaseProductQuantityRaw(userId, productId);
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
            if (userId <= INVALID_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }

            if (basketId <= INVALID_BASKET_ID)
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

                // Create request for the account API to create orders from the basket
                var request = new
                {
                    BasketId = basketId,
                    DiscountAmount = basketTotals.Discount,
                    TotalAmount = basketTotals.TotalAmount
                };

                System.Diagnostics.Debug.WriteLine($"Checkout request: BasketId={basketId}, Discount={basketTotals.Discount}, Total={basketTotals.TotalAmount}");

                // Call the account API to create the order
                var response = await basketRepository.CheckoutBasketRaw(userId, basketId, request);

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