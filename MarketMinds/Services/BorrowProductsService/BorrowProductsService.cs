using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration;
using MarketMinds.Repositories;
using MarketMinds.Services.ProductTagService;

namespace MarketMinds.Services.BorrowProductsService;

// DTO class that matches the server's expectations
public class CreateBorrowProductDTO
{
    // Use JsonPropertyName to ensure exact JSON property names match
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("sellerId")]
    public int SellerId { get; set; }
    [JsonPropertyName("conditionId")]
    public int? ConditionId { get; set; }
    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }
    [JsonPropertyName("timeLimit")]
    public DateTime TimeLimit { get; set; }
    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }
    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }
    [JsonPropertyName("dailyRate")]
    public double DailyRate { get; set; }
    [JsonPropertyName("isBorrowed")]
    public bool IsBorrowed { get; set; }
    [JsonPropertyName("images")]
    public List<ImageInfo> Images { get; set; } = new List<ImageInfo>();

    public class ImageInfo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}

public class BorrowProductsService : ProductService, IBorrowProductsService
{
    private readonly HttpClient httpClient;
    private readonly string apiBaseUrl;

    public BorrowProductsService(IConfiguration configuration) : base(null)
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

    public BorrowProductsService(IProductsRepository repository) : base(repository)
    {
        // This constructor doesn't initialize httpClient and shouldn't be used for API calls
        httpClient = new HttpClient();
        apiBaseUrl = "http://localhost:5000/";
        httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        Console.WriteLine("Warning: Using repository constructor but initializing HTTP client with default values");
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

        // Creating a proper DTO for sending to the API
        var productDTO = new CreateBorrowProductDTO
        {
            Title = borrowProduct.Title,
            Description = borrowProduct.Description,
            SellerId = borrowProduct.Seller?.Id ?? 0,
            ConditionId = borrowProduct.Condition?.Id ?? 0,
            CategoryId = borrowProduct.Category?.Id ?? 0,
            TimeLimit = borrowProduct.TimeLimit,
            StartDate = borrowProduct.StartDate,
            EndDate = borrowProduct.EndDate,
            DailyRate = borrowProduct.DailyRate,
            IsBorrowed = borrowProduct.IsBorrowed,
            Images = borrowProduct.Images == null
                ? new List<CreateBorrowProductDTO.ImageInfo>()
                : borrowProduct.Images.Select(img => new CreateBorrowProductDTO.ImageInfo { Url = img.Url }).ToList()
        };

        Console.WriteLine($"Sending product payload: {System.Text.Json.JsonSerializer.Serialize(productDTO)}");
        Console.WriteLine($"Sending to URL: {httpClient.BaseAddress}borrowproducts");
        try
        {
            // Log request details
            Console.WriteLine("=== REQUEST DIAGNOSTICS ===");
            Console.WriteLine($"HttpClient DefaultRequestHeaders: {string.Join(", ", httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}:{string.Join(",", h.Value)}"))}");
            Console.WriteLine($"HttpClient BaseAddress: {httpClient.BaseAddress}");
            // Create a custom JsonSerializerOptions to see exactly what's being sent
            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            // Diagnose the exact payload we're trying to send
            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(productDTO, serializerOptions);
            Console.WriteLine("Actual JSON payload being sent:");
            Console.WriteLine(jsonPayload);
            Console.WriteLine("===========================");
            // Use JsonContent instead of PostAsJsonAsync to have more control over serialization
            var content = System.Net.Http.Json.JsonContent.Create(productDTO, null, serializerOptions);
            var response = httpClient.PostAsync("borrowproducts", content).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                // Check if this is a validation error and provide more helpful details
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("Validation error detected. Ensure navigation properties are handled correctly:");
                    Console.WriteLine("- Seller, Category, and Condition should be null but their IDs should be set");
                    Console.WriteLine("- Images should be sent as URLs only without nested objects");
                }
                response.EnsureSuccessStatusCode(); // This will throw with the status code
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

    public override List<Product> GetProducts()
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

    public override Product GetProductById(int id)
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