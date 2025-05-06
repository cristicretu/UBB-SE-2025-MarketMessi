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
            Console.WriteLine($"Initialized HTTP client with base address: {httpClient.BaseAddress}");
        }

        public void CreateListing(Product product)
        {
            if (!(product is BorrowProduct borrowProduct))
            {
                throw new ArgumentException("Product must be a BorrowProduct.", nameof(product));
            }

            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            Console.WriteLine($"Start Date: {borrowProduct.StartDate}");
            Console.WriteLine($"End Date: {borrowProduct.EndDate}");
            // Ensure valid dates
            if (borrowProduct.StartDate == default(DateTime))
            {
                borrowProduct.StartDate = DateTime.Now;
                Console.WriteLine($"Setting default StartDate to now: {borrowProduct.StartDate}");
            }
            if (borrowProduct.EndDate == default(DateTime))
            {
                borrowProduct.EndDate = DateTime.Now.AddDays(7);
                Console.WriteLine($"Setting default EndDate to 7 days from now: {borrowProduct.EndDate}");
            }
            // Fix for TimeLimit default value
            if (borrowProduct.TimeLimit == default(DateTime))
            {
                borrowProduct.TimeLimit = DateTime.Now.AddDays(7);
                Console.WriteLine($"Setting default TimeLimit to 7 days from now: {borrowProduct.TimeLimit}");
            }
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
            Console.WriteLine($"Sending product payload: {System.Text.Json.JsonSerializer.Serialize(productToSend)}");
            Console.WriteLine($"Sending to URL: {httpClient.BaseAddress}borrowproducts");
            try
            {
                // Use a more direct approach to sending the payload
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
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"API Success Response: {responseContent}");
                }
                Console.WriteLine("Successfully created borrow product listing.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during API call: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public void DeleteListing(Product product)
        {
            if (product.Id == 0)
            {
                throw new ArgumentException("Product ID must be set for delete.", nameof(product.Id));
            }
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }
            var response = httpClient.DeleteAsync($"borrowproducts/{product.Id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public List<Product> GetProducts()
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            try
            {
                // Create serializer options that are more lenient for deserialization
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializerOptions.Converters.Add(new UserJsonConverter());

                // Get the response as a string first to handle deserialization manually if needed
                var response = httpClient.GetAsync("borrowproducts").Result;
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Received JSON from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));
                var products = System.Text.Json.JsonSerializer.Deserialize<List<BorrowProduct>>(json, serializerOptions);
                return products?.Cast<Product>().ToList() ?? new List<Product>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting products: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                // Return empty list instead of throwing to avoid cascading failures
                return new List<Product>();
            }
        }

        public Product GetProductById(int id)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            try
            {
                // Create serializer options that are more lenient for deserialization
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializerOptions.Converters.Add(new UserJsonConverter());

                // Get the response as a string first to handle deserialization manually if needed
                var response = httpClient.GetAsync($"borrowproducts/{id}").Result;
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Received JSON for product {id} from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));
                var product = System.Text.Json.JsonSerializer.Deserialize<BorrowProduct>(json, serializerOptions);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Borrow product with ID {id} not found.");
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
                throw new KeyNotFoundException($"Borrow product with ID {id} not found: {ex.Message}");
            }
        }
    }
}