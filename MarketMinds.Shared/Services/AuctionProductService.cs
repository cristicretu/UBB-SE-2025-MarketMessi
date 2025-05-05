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
        private const int MAX_AUCTION_EXTENSION_MINUTES = 5;
        private const double DEFAULT_MIN_PRICE = 1.0;

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
                // Apply business logic before sending to API
                SetDefaultAuctionTimes(auctionProduct);
                SetDefaultPricing(auctionProduct);
                
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
                // First, get the auction product to apply business logic
                var auction = await GetAuctionProductByIdAsync(auctionId);
                if (auction == null || auction.Id == 0)
                {
                    throw new KeyNotFoundException($"Auction product with ID {auctionId} not found.");
                }
                
                // Validate the bid
                ValidateBid(auction, bidderId, bidAmount);
                
                // Process refund for previous bidder if any
                ProcessRefundForPreviousBidder(auction, bidAmount);
                
                // Extend auction time if needed
                ExtendAuctionTimeIfNeeded(auction);
                
                // Update auction with new bid and time if it was extended
                if (auction.EndTime > DateTime.Now)
                {
                    // Make the actual API call to place the bid
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
                else
                {
                    // Auction has ended
                    throw new InvalidOperationException("Auction has already ended.");
                }
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

        // Business logic methods
        public void ValidateBid(AuctionProduct auction, int bidderId, double bidAmount)
        {
            // Check if auction has ended
            if (IsAuctionEnded(auction))
            {
                throw new InvalidOperationException("Cannot place bid on an ended auction.");
            }
            
            // Determine minimum bid amount
            double minBidAmount;
            if (auction.Bids == null || !auction.Bids.Any())
            {
                // No bids yet, use start price
                minBidAmount = auction.StartPrice;
            }
            else
            {
                // Existing bids, new bid must be higher than current price
                minBidAmount = auction.CurrentPrice + 1;
            }
            
            // Check if bid amount is sufficient
            if (bidAmount < minBidAmount)
            {
                throw new InvalidOperationException($"Bid must be at least ${minBidAmount}");
            }
            
            // if (user.Balance < bidAmount)
            // {
            //     throw new InvalidOperationException("Insufficient balance to place this bid.");
            // }
        }

        public void ExtendAuctionTimeIfNeeded(AuctionProduct auction)
        {
            // If auction is ending soon and a new bid comes in, extend the time
            var timeRemaining = auction.EndTime - DateTime.Now;
            
            if (timeRemaining.TotalMinutes < MAX_AUCTION_EXTENSION_MINUTES)
            {
                var oldEndTime = auction.EndTime;
                auction.EndTime = DateTime.Now.AddMinutes(MAX_AUCTION_EXTENSION_MINUTES);
                Console.WriteLine($"Extended auction {auction.Id} end time from {oldEndTime} to {auction.EndTime}");
            }
        }

        public void SetDefaultAuctionTimes(AuctionProduct product)
        {
            if (product.StartTime == default || product.StartTime.Year < 2000)
            {
                product.StartTime = DateTime.Now;
            }
            
            if (product.EndTime == default || product.EndTime.Year < 2000)
            {
                product.EndTime = DateTime.Now.AddDays(7);
            }
        }

        public void SetDefaultPricing(AuctionProduct product)
        {
            if (product.StartPrice <= 0)
            {
                if (product.CurrentPrice > 0)
                {
                    product.StartPrice = product.CurrentPrice;
                }
                else
                {
                    product.StartPrice = DEFAULT_MIN_PRICE;
                    product.CurrentPrice = DEFAULT_MIN_PRICE;
                }
            }
        }

        public void ProcessRefundForPreviousBidder(AuctionProduct product, double newBidAmount)
        {
            // In a real implementation, this would contact a user service to refund the previous bidder
            if (product.Bids != null && product.Bids.Any())
            {
                var highestBid = product.Bids.OrderByDescending(b => b.Price).FirstOrDefault();
                if (highestBid != null)
                {
                    // Simulated code to refund the previous bidder
                    int previousBidderId = highestBid.BidderId;
                    double previousBidAmount = (double)highestBid.Price;
                    
                    Console.WriteLine($"Would refund previous bidder (ID:{previousBidderId}) their bid amount of ${previousBidAmount}");
                    
                    // In real implementation:
                    // await _userService.RefundBidAmount(previousBidderId, previousBidAmount);
                }
            }
        }

        public bool IsAuctionEnded(AuctionProduct auction)
        {
            return DateTime.Now > auction.EndTime;
        }
    }
} 