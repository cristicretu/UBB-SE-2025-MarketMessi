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
        private readonly BasketProxyRepository proxyRepository;
        private readonly JsonSerializerOptions jsonOptions;

        // Constructor with configuration
        public BasketService(IBasketRepository basketRepository)
        {
            this.basketRepository = basketRepository;
            this.proxyRepository = basketRepository as BasketProxyRepository;

            if (this.proxyRepository == null && basketRepository.GetType().Name.Contains("Proxy"))
            {
                throw new InvalidOperationException("Expected BasketProxyRepository but received incompatible type.");
            }

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
                if (proxyRepository != null)
                {
                    proxyRepository.AddProductToBasketRaw(userId, productId, limitedQuantity);
                }
                else
                {
                    // If not a proxy repository, use the standard interface method
                    basketRepository.AddItemToBasket(userId, productId, limitedQuantity);
                }
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
                if (proxyRepository != null)
                {
                    // Use the proxy repository to get the raw JSON response
                    var responseJson = proxyRepository.GetBasketByUserRaw(user.Id);

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
                else
                {
                    // Use the standard interface method
                    return basketRepository.GetBasketByUserId(user.Id);
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
                if (proxyRepository != null)
                {
                    // Make the API call to remove product from basket
                    proxyRepository.RemoveProductFromBasketRaw(userId, productId);
                }
                else
                {
                    // Use the standard interface method
                    basketRepository.RemoveItemByProductId(userId, productId);
                }
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

                if (proxyRepository != null)
                {
                    // Make the API call to update product quantity
                    proxyRepository.UpdateProductQuantityRaw(userId, productId, limitedQuantity);
                }
                else
                {
                    // Use the standard interface method
                    basketRepository.UpdateItemQuantityByProductId(userId, productId, limitedQuantity);
                }
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
                if (proxyRepository != null)
                {
                    // Clear the basket through API
                    proxyRepository.ClearBasketRaw(userId);
                }
                else
                {
                    // Use the standard interface method
                    basketRepository.ClearBasket(userId);
                }
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
                if (proxyRepository != null)
                {
                    // Validate basket through API
                    var responseContent = proxyRepository.ValidateBasketBeforeCheckOutRaw(basketId);

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
                else
                {
                    // Default validation logic when not using proxy
                    var items = basketRepository.GetBasketItems(basketId);
                    return items != null && items.Count > 0;
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
                if (proxyRepository != null)
                {
                    // Apply promo code through API
                    var response = proxyRepository.ApplyPromoCodeRaw(basketId, code);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException("Invalid promo code");
                    }
                }
                else
                {
                    // Default behavior for non-proxy implementations
                    // No-op, as non-proxy implementations may not support promo codes
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
                if (proxyRepository != null)
                {
                    var responseJson = proxyRepository.GetPromoCodeDiscountRaw(code);

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
                else
                {
                    // Default behavior for non-proxy implementations
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
                if (proxyRepository != null)
                {
                    // Get totals from API
                    var responseJson = proxyRepository.CalculateBasketTotalsRaw(basketId, promoCode);

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
                else
                {
                    // Calculate totals manually for non-proxy implementations
                    var totals = new BasketTotals();
                    var items = basketRepository.GetBasketItems(basketId);

                    if (items != null)
                    {
                        totals.Subtotal = items.Sum(i => i.Price * i.Quantity);

                        // Apply discount if promo code provided
                        if (!string.IsNullOrEmpty(promoCode))
                        {
                            totals.Discount = GetPromoCodeDiscount(promoCode, totals.Subtotal);
                        }

                        totals.TotalAmount = totals.Subtotal - totals.Discount;
                    }

                    return totals;
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
                if (proxyRepository != null)
                {
                    // Decrease quantity through API
                    proxyRepository.DecreaseProductQuantityRaw(userId, productId);
                }
                else
                {
                    // For non-proxy implementations, get current quantity and decrease by 1
                    var basket = basketRepository.GetBasketByUserId(userId);
                    var item = basket?.Items?.FirstOrDefault(i => i.ProductId == productId);

                    if (item != null && item.Quantity > 1)
                    {
                        basketRepository.UpdateItemQuantityByProductId(userId, productId, item.Quantity - 1);
                    }
                    else if (item != null && item.Quantity == 1)
                    {
                        basketRepository.RemoveItemByProductId(userId, productId);
                    }
                }
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
                if (proxyRepository != null)
                {
                    // Increase quantity through API
                    proxyRepository.IncreaseProductQuantityRaw(userId, productId);
                }
                else
                {
                    // For non-proxy implementations, get current quantity and increase by 1
                    var basket = basketRepository.GetBasketByUserId(userId);
                    var item = basket?.Items?.FirstOrDefault(i => i.ProductId == productId);

                    if (item != null)
                    {
                        int newQuantity = Math.Min(item.Quantity + 1, MAXIMUM_QUANTITY_PER_ITEM);
                        basketRepository.UpdateItemQuantityByProductId(userId, productId, newQuantity);
                    }
                    else
                    {
                        // If item doesn't exist, add it with quantity 1
                        basketRepository.AddItemToBasket(userId, productId, 1);
                    }
                }
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

                if (proxyRepository != null)
                {
                    // Call the account API to create the order
                    var response = await proxyRepository.CheckoutBasketRaw(userId, basketId, request);

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
                else
                {
                    // For non-proxy implementations, we'd need a different checkout mechanism
                    // This is a placeholder - in a real implementation, you'd process the order locally
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

        // New methods for the standard IBasketRepository operations with validation
        public Basket GetBasketByUserId(int userId)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }

            try
            {
                return basketRepository.GetBasketByUserId(userId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve user's basket: {ex.Message}", ex);
            }
        }

        public void RemoveItemByProductId(int basketId, int productId)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                basketRepository.RemoveItemByProductId(basketId, productId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to remove item from basket: {ex.Message}", ex);
            }
        }

        public List<BasketItem> GetBasketItems(int basketId)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                return basketRepository.GetBasketItems(basketId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve basket items: {ex.Message}", ex);
            }
        }

        public void AddItemToBasket(int basketId, int productId, int quantity)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
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

                basketRepository.AddItemToBasket(basketId, productId, limitedQuantity);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to add item to basket: {ex.Message}", ex);
            }
        }

        public void UpdateItemQuantityByProductId(int basketId, int productId, int quantity)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
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

                basketRepository.UpdateItemQuantityByProductId(basketId, productId, limitedQuantity);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to update item quantity: {ex.Message}", ex);
            }
        }
    }
}