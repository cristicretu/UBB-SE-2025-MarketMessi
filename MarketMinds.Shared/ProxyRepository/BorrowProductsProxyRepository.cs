using System.Net.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;

namespace MarketMinds.Shared.ProxyRepository
{
    public class BorrowProductsProxyRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public BorrowProductsProxyRepository(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            }
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public void CreateListing(Product product)
        {
            BorrowProduct borrowProduct = product as BorrowProduct;
            var productToSend = new
            {
                borrowProduct.Title,
                borrowProduct.Description,
                SellerId = borrowProduct.Seller?.Id ?? 0,
                ConditionId = borrowProduct.Condition?.Id,
                CategoryId = borrowProduct.Category?.Id,
                borrowProduct.DailyRate,
                StartDate = borrowProduct.StartDate,
                EndDate = borrowProduct.EndDate,
                Images = borrowProduct.Images == null || !borrowProduct.Images.Any()
                       ? (borrowProduct.NonMappedImages != null && borrowProduct.NonMappedImages.Any()
                          ? borrowProduct.NonMappedImages.Select(img => new { Url = img.Url ?? string.Empty }).Cast<object>().ToList()
                          : new List<object>())
                       : borrowProduct.Images.Select(img => new { img.Url }).Cast<object>().ToList()
            };

            try
            {
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                var content = System.Net.Http.Json.JsonContent.Create(productToSend, null, serializerOptions);
                var response = httpClient.PostAsync("borrowproducts", content).Result;
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
            catch (Exception exception)
            {
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {exception.InnerException.GetType().Name} - {exception.InnerException.Message}");
                }
                throw;
            }
        }

        public void DeleteListing(Product product)
        {
            var response = httpClient.DeleteAsync($"borrowproducts/{product.Id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public List<Product> GetProducts()
        {
            try
            {
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializerOptions.Converters.Add(new UserJsonConverter());

                var response = httpClient.GetAsync("borrowproducts").Result;
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
            
                var products = System.Text.Json.JsonSerializer.Deserialize<List<BorrowProduct>>(json, serializerOptions);
                return products?.Cast<Product>().ToList() ?? new List<Product>();
            }
            catch (Exception exception)
            {
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {exception.InnerException.Message}");
                }
                return new List<Product>();
            }
        }

        public Product GetProductById(int id)
        {
            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            serializerOptions.Converters.Add(new UserJsonConverter());

            var response = httpClient.GetAsync($"borrowproducts/{id}").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
         
            var product = System.Text.Json.JsonSerializer.Deserialize<BorrowProduct>(json, serializerOptions);
            return product;
        }
    }
}