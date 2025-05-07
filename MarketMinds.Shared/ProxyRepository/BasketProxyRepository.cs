using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Shared.ProxyRepository
{
    public class BasketProxyRepository : IBasketRepository
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

        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        // Constructor with configuration
        public BasketProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
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

        public HttpResponseMessage AddProductToBasketRaw(int userId, int productId, int quantity)
        {
            var response = httpClient.PostAsJsonAsync($"user/{userId}/product/{productId}", quantity).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        public string GetBasketByUserRaw(int userId)
        {
            string fullUrl = $"{httpClient.BaseAddress.AbsoluteUri.Replace("api/basket/", "")}api/basket/user/{userId}";
            var response = httpClient.GetAsync(fullUrl).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public HttpResponseMessage RemoveProductFromBasketRaw(int userId, int productId)
        {
            var response = httpClient.DeleteAsync($"user/{userId}/product/{productId}").Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        public HttpResponseMessage UpdateProductQuantityRaw(int userId, int productId, int quantity)
        {
            var response = httpClient.PutAsJsonAsync($"user/{userId}/product/{productId}", quantity).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        public HttpResponseMessage ClearBasketRaw(int userId)
        {
            var response = httpClient.DeleteAsync($"user/{userId}/clear").Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        public string ValidateBasketBeforeCheckOutRaw(int basketId)
        {
            var response = httpClient.GetAsync($"{basketId}/validate").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public HttpResponseMessage ApplyPromoCodeRaw(int basketId, string code)
        {
            var response = httpClient.PostAsJsonAsync($"{basketId}/promocode", code).Result;
            return response;
        }

        public string GetPromoCodeDiscountRaw(string code)
        {
            string normalizedCode = code.Trim().ToUpper();
            var response = httpClient.PostAsJsonAsync($"1/promocode", normalizedCode).Result;

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            return "{}";
        }

        public string CalculateBasketTotalsRaw(int basketId, string promoCode)
        {
            string endpoint = $"{basketId}/totals";
            if (!string.IsNullOrEmpty(promoCode))
            {
                endpoint += $"?promoCode={promoCode}";
            }

            var response = httpClient.GetAsync(endpoint).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public HttpResponseMessage DecreaseProductQuantityRaw(int userId, int productId)
        {
            var response = httpClient.PutAsync($"user/{userId}/product/{productId}/decrease", null).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        public HttpResponseMessage IncreaseProductQuantityRaw(int userId, int productId)
        {
            var response = httpClient.PutAsync($"user/{userId}/product/{productId}/increase", null).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        public async Task<HttpResponseMessage> CheckoutBasketRaw(int userId, int basketId, object requestData)
        {
            using var httpClientAccount = new HttpClient();
            httpClientAccount.BaseAddress = new Uri(httpClient.BaseAddress.AbsoluteUri.Replace("api/basket/", ""));
            httpClientAccount.Timeout = TimeSpan.FromSeconds(30);

            return await httpClientAccount.PostAsJsonAsync($"api/account/{userId}/orders", requestData);
        }

        // Legacy methods that throw NotImplementedException
        public void AddItemToBasket(int basketId, int productId, int quantity)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void ClearBasket(int basketId)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public List<BasketItem> GetBasketItems(int basketId)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public Basket GetBasketByUserId(int userId)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void RemoveItemByProductId(int basketId, int productId)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void UpdateItemQuantityByProductId(int basketId, int productId, int quantity)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }
    }
}