using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration;
using MarketMinds.Services.ProductTagService;

namespace MarketMinds.Services.BuyProductsService
{
    public class BuyProductsService : ProductService, IBuyProductsService
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public BuyProductsService(IConfiguration configuration) : base(null)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public override List<Product> GetProducts()
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

        public void CreateListing(Product product)
        {
            if (!(product is BuyProduct buyProduct))
            {
                throw new ArgumentException("Product must be a BuyProduct.", nameof(product));
            }
            var productToSend = new
            {
                buyProduct.Title,
                buyProduct.Description,
                SellerId = buyProduct.Seller?.Id ?? 0,
                ConditionId = buyProduct.Condition?.Id,
                CategoryId = buyProduct.Category?.Id,
                buyProduct.Price,
                Images = buyProduct.Images == null
                       ? new List<object>()
                       : buyProduct.Images.Select(img => new { img.Url }).Cast<object>().ToList()
            };
            var response = httpClient.PostAsJsonAsync("buyproducts", productToSend).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                throw new HttpRequestException($"Failed to create listing. Status: {response.StatusCode}, Error: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
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

        public override Product GetProductById(int id)
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