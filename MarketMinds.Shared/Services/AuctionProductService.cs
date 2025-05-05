using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.Services
{
    public class NumberToStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return string.Empty;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64().ToString();
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }

            throw new JsonException($"Unable to convert {reader.TokenType} to string");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value ?? string.Empty);
        }
    }

    public class AuctionProductService : IAuctionProductService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuctionProductService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            
            // Configure base address if it's not already set
            if (_httpClient.BaseAddress == null)
            {
                var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/api/";
                _httpClient.BaseAddress = new Uri(apiBaseUrl);
            }
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };
            
            // Add custom converter for handling password property
            _jsonOptions.Converters.Add(new NumberToStringConverter());
        }

        public async Task<List<AuctionProduct>> GetAllAuctionProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/auctionproducts");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var auctionProducts = JsonSerializer.Deserialize<List<AuctionProduct>>(content, _jsonOptions);
                return auctionProducts ?? new List<AuctionProduct>();
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error getting auction products: {ex.Message}");
                return new List<AuctionProduct>();
            }
        }

        public async Task<AuctionProduct> GetAuctionProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/auctionproducts/{id}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var auctionProduct = JsonSerializer.Deserialize<AuctionProduct>(content, _jsonOptions);
                return auctionProduct ?? new AuctionProduct();
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error getting auction product by id {id}: {ex.Message}");
                return new AuctionProduct();
            }
        }

        public async Task<bool> CreateAuctionProductAsync(AuctionProduct auctionProduct)
        {
            try
            {
                // Validate and prepare the product for creation
                if (auctionProduct.StartTime == default)
                {
                    auctionProduct.StartTime = DateTime.Now;
                }
                
                if (auctionProduct.EndTime == default)
                {
                    auctionProduct.EndTime = DateTime.Now.AddDays(7);
                }
                
                var productToSend = new
                {
                    auctionProduct.Title,
                    auctionProduct.Description,
                    SellerId = auctionProduct.Seller?.Id ?? 0,
                    ConditionId = auctionProduct.Condition?.Id,
                    CategoryId = auctionProduct.Category?.Id,
                    auctionProduct.StartTime,
                    auctionProduct.EndTime,
                    auctionProduct.StartPrice,
                    auctionProduct.CurrentPrice,
                    Images = auctionProduct.Images?.Select(img => new { img.Url }).ToList()
                };

                var response = await _httpClient.PostAsJsonAsync("api/auctionproducts", productToSend);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error creating auction product: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PlaceBidAsync(int auctionId, int bidderId, double bidAmount)
        {
            try
            {
                var bidToSend = new
                {
                    ProductId = auctionId,
                    BidderId = bidderId,
                    Amount = bidAmount,
                    Timestamp = DateTime.Now
                };
                
                var response = await _httpClient.PostAsJsonAsync($"api/auctionproducts/{auctionId}/bids", bidToSend);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error placing bid: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAuctionProductAsync(AuctionProduct auctionProduct)
        {
            try
            {
                var productToSend = new
                {
                    auctionProduct.Id,
                    auctionProduct.Title,
                    auctionProduct.Description,
                    SellerId = auctionProduct.Seller?.Id,
                    ConditionId = auctionProduct.Condition?.Id,
                    CategoryId = auctionProduct.Category?.Id,
                    auctionProduct.StartTime,
                    auctionProduct.EndTime,
                    auctionProduct.StartPrice,
                    auctionProduct.CurrentPrice,
                    Images = auctionProduct.Images?.Select(img => new { img.Url }).ToList()
                };

                var response = await _httpClient.PutAsJsonAsync($"api/auctionproducts/{auctionProduct.Id}", productToSend);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error updating auction product: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAuctionProductAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/auctionproducts/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error deleting auction product: {ex.Message}");
                return false;
            }
        }
    }
} 