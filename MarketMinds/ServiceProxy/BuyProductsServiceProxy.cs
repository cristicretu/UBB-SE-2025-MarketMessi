using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Services;
using MarketMinds.Shared.Models;

namespace MarketMinds.ServiceProxy
{
    public class BuyProductsServiceProxy
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public BuyProductsServiceProxy(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public List<Product> GetProducts()
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            try
            {
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializerOptions.Converters.Add(new UserJsonConverter());

                var response = httpClient.GetAsync("buyproducts").Result;
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Received JSON from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));
                var products = System.Text.Json.JsonSerializer.Deserialize<List<BuyProduct>>(json, serializerOptions);
                return products?.Cast<Product>().ToList() ?? new List<Product>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting products: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return new List<Product>();
            }
        }

        public void CreateListing(BuyProduct product)
        {
            var productToSend = new
            {
                product.Title,
                product.Description,
                SellerId = product.Seller?.Id ?? 0,
                ConditionId = product.Condition?.Id,
                CategoryId = product.Category?.Id,
                product.Price,
                Images = product.Images == null || !product.Images.Any()
                       ? (product.NonMappedImages != null && product.NonMappedImages.Any() 
                          ? product.NonMappedImages.Select(img => new { Url = img.Url ?? "" }).Cast<object>().ToList()
                          : new List<object>())
                       : product.Images.Select(img => new { img.Url }).Cast<object>().ToList()
            };
            var response = httpClient.PostAsJsonAsync("buyproducts", productToSend).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                throw new HttpRequestException($"Failed to create listing. Status: {response.StatusCode}, Error: {errorContent}");
            }
            else
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
            }
        }

        public void DeleteListing(Product product)
        {
            if (product.Id == 0)
            {
                throw new ArgumentException("Product ID must be set for delete.", nameof(product.Id));
            }
            var response = httpClient.DeleteAsync($"buyproducts/{product.Id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public BuyProduct GetProductById(int id)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            try
            {
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializerOptions.Converters.Add(new UserJsonConverter());

                var response = httpClient.GetAsync($"buyproducts/{id}").Result;
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Received JSON for product {id} from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));
                var product = System.Text.Json.JsonSerializer.Deserialize<BuyProduct>(json, serializerOptions);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Buy product with ID {id} not found.");
                }
                return product;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product by ID {id}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw new KeyNotFoundException($"Buy product with ID {id} not found: {ex.Message}");
            }
        }
    }
}