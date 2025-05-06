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

namespace MarketMinds.ServiceProxy
{
    public class AuctionProductsServiceProxy
    {
        private const int NULL_BID_AMOUNT = 0;
        private const int MAX_AUCTION_TIME = 5;
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public AuctionProductsServiceProxy(IConfiguration configuration)
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
            if (auctionProduct.StartTime == default(DateTime))
            {
                auctionProduct.StartTime = DateTime.Now;
            }
            if (auctionProduct.EndTime == default(DateTime))
            {
                auctionProduct.EndTime = DateTime.Now.AddDays(7);
            }
            if (auctionProduct.StartPrice <= 0 && auctionProduct.CurrentPrice > 0)
            {
                auctionProduct.StartPrice = auctionProduct.CurrentPrice;
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

            if (auctionProduct.Images != null && auctionProduct.Images.Any())
            {
                foreach (var img in auctionProduct.Images)
                {
                    Console.WriteLine($"Image URL from Images: {img.Url}");
                }
            }
            if (auctionProduct.NonMappedImages != null && auctionProduct.NonMappedImages.Any())
            {
                foreach (var img in auctionProduct.NonMappedImages)
                {
                    Console.WriteLine($"Image URL from NonMappedImages: {img.Url}");
                }
            }

            Console.WriteLine($"Sending product payload: {System.Text.Json.JsonSerializer.Serialize(productToSend)}");

            var response = httpClient.PostAsJsonAsync("auctionproducts", productToSend).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                response.EnsureSuccessStatusCode();
            }
            else
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
            }
        }

        public void PlaceBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            try
            {
                ValidateBid(auction, bidder, bidAmount);
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
                    Console.WriteLine($"API Error when placing bid: {response.StatusCode} - {errorContent}");
                    response.EnsureSuccessStatusCode();
                    return;
                }
                bidder.Balance -= bidAmount;
                var bid = new Bid(bidder.Id, auction.Id, bidAmount)
                {
                    Product = auction,
                    Bidder = bidder
                };
                auction.AddBid(bid);
                auction.CurrentPrice = bidAmount;
                ExtendAuctionTime(auction);
                Console.WriteLine($"Bid of ${bidAmount} successfully placed on auction {auction.Id}");
            }
            catch (Exception bidPlacementException)
            {
                Console.WriteLine($"Error placing bid: {bidPlacementException.Message}");
                throw;
            }
        }

        public void ConcludeAuction(AuctionProduct auction)
        {
            if (auction.Id == 0)
            {
                throw new ArgumentException("Auction Product ID must be set for delete.", nameof(auction.Id));
            }
            var response = httpClient.DeleteAsync($"auctionproducts/{auction.Id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public void ValidateBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            double minBid = auction.Bids.Count == NULL_BID_AMOUNT ? auction.StartPrice : auction.CurrentPrice + 1;

            if (bidAmount < minBid)
            {
                throw new Exception($"Bid must be at least ${minBid}");
            }

            if (bidAmount > bidder.Balance)
            {
                throw new Exception("Insufficient balance");
            }

            if (DateTime.Now > auction.EndTime)
            {
                throw new Exception("Auction already ended");
            }
        }

        public void ExtendAuctionTime(AuctionProduct auction)
        {
            var timeRemaining = auction.EndTime - DateTime.Now;

            if (timeRemaining.TotalMinutes < MAX_AUCTION_TIME)
            {
                var oldEndTime = auction.EndTime;
                auction.EndTime = DateTime.Now.AddMinutes(MAX_AUCTION_TIME);
            }
        }

        public List<AuctionProduct> GetProducts()
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

                var response = httpClient.GetAsync("auctionproducts").Result;
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Received JSON from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));
                var products = System.Text.Json.JsonSerializer.Deserialize<List<AuctionProduct>>(json, serializerOptions);
                return products ?? new List<AuctionProduct>();
            }
            catch (Exception getProductsException)
            {
                Console.WriteLine($"Error getting products: {getProductsException.Message}");
                if (getProductsException.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {getProductsException.InnerException.Message}");
                }
                return new List<AuctionProduct>();
            }
        }

        public AuctionProduct GetProductById(int id)
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

                var response = httpClient.GetAsync($"auctionproducts/{id}").Result;
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Received JSON for product {id} from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));
                var product = System.Text.Json.JsonSerializer.Deserialize<AuctionProduct>(json, serializerOptions);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Auction product with ID {id} not found.");
                }
                return product;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception getProductByIdException)
            {
                Console.WriteLine($"Error getting product by ID {id}: {getProductByIdException.Message}");
                if (getProductByIdException.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {getProductByIdException.InnerException.Message}");
                }
                throw new KeyNotFoundException($"Auction product with ID {id} not found: {getProductByIdException.Message}");
            }
        }
    }
}
