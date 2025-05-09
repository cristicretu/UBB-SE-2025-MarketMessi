using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;

namespace MarketMinds.Shared.ProxyRepository
{
    public class AuctionProductsProxyRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public AuctionProductsProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public void CreateListing(Product product)
        {
            if (!(product is AuctionProduct auctionProduct))
            {
                throw new ArgumentException("Product must be an AuctionProduct.", nameof(product));
            }
            
            var productToSend = new
            {
                auctionProduct.Title,
                auctionProduct.Description,
                SellerId = auctionProduct.Seller?.Id ?? 0,
                ConditionId = auctionProduct.Condition?.Id,
                CategoryId = auctionProduct.Category?.Id,
                StartTime = auctionProduct.StartTime,
                EndTime = auctionProduct.EndTime,
                StartPrice = auctionProduct.StartPrice,
                CurrentPrice = auctionProduct.CurrentPrice,
                Images = auctionProduct.Images == null || !auctionProduct.Images.Any()
                       ? (auctionProduct.NonMappedImages != null && auctionProduct.NonMappedImages.Any()
                          ? auctionProduct.NonMappedImages.Select(img => new { Url = img.Url ?? string.Empty }).Cast<object>().ToList()
                          : new List<object>())
                       : auctionProduct.Images.Select(img => new { img.Url }).Cast<object>().ToList()
            };

            var response = httpClient.PostAsJsonAsync("auctionproducts", productToSend).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
            }
            else
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
            }
        }

        public void PlaceBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            var bidToSend = new
            {
                ProductId = auction.Id,
                BidderId = bidder.Id,
                Amount = bidAmount,
                Timestamp = DateTime.Now
            };
            
            var response = httpClient.PostAsJsonAsync($"auctionproducts/{auction.Id}/bids", bidToSend).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
            }
        }

        public void ConcludeAuction(AuctionProduct auction)
        {
            var response = httpClient.DeleteAsync($"auctionproducts/{auction.Id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public List<AuctionProduct> GetProducts()
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            serializerOptions.Converters.Add(new UserJsonConverter());

            var response = httpClient.GetAsync("auctionproducts").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var products = System.Text.Json.JsonSerializer.Deserialize<List<AuctionProduct>>(json, serializerOptions);
            return products ?? new List<AuctionProduct>();
        }

        public AuctionProduct GetProductById(int id)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            serializerOptions.Converters.Add(new UserJsonConverter());

            var response = httpClient.GetAsync($"auctionproducts/{id}").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var product = System.Text.Json.JsonSerializer.Deserialize<AuctionProduct>(json, serializerOptions);
            return product;
        }
    }
}
