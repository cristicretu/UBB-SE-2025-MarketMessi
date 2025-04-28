using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Services.ProductTagService;
using MarketMinds.Repositories;

namespace MarketMinds.Services.AuctionProductsService
{
    public class AuctionProductsService : ProductService, IAuctionProductsService
    {
        private const int NULL_BID_AMOUNT = 0;
        private const int MAX_AUCTION_TIME = 5;
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public AuctionProductsService(IConfiguration configuration) : base(null)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public AuctionProductsService(IProductsRepository repository) : base(repository)
        {
        }

        public void CreateListing(Product product)
        {
            var auctionProduct = product as AuctionProduct;
            if (auctionProduct == null)
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
                Images = auctionProduct.Images == null
                       ? new List<object>()
                       : auctionProduct.Images.Select(img => new { img.Url }).Cast<object>().ToList()
            };
            Console.WriteLine($"Sending product payload: {System.Text.Json.JsonSerializer.Serialize(productToSend)}");
            var response = httpClient.PostAsJsonAsync("auctionproducts", productToSend).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                response.EnsureSuccessStatusCode();
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
                var bid = new Bid 
                { 
                    BidderId = bidder.Id,
                    Bidder = bidder,
                    Price = bidAmount,
                    Timestamp = DateTime.Now
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

        public string GetTimeLeft(AuctionProduct auction)
        {
            var timeLeft = auction.EndTime - DateTime.Now;
            return timeLeft > TimeSpan.Zero ? timeLeft.ToString(@"dd\:hh\:mm\:ss") : "Auction Ended";
        }

        public bool IsAuctionEnded(AuctionProduct auction)
        {
            return DateTime.Now >= auction.EndTime;
        }

        private void ValidateBid(AuctionProduct auction, User bidder, double bidAmount)
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

        private void ExtendAuctionTime(AuctionProduct auction)
        {
            var timeRemaining = auction.EndTime - DateTime.Now;

            if (timeRemaining.TotalMinutes < MAX_AUCTION_TIME)
            {
                auction.EndTime = DateTime.Now.AddMinutes(MAX_AUCTION_TIME);
            }
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

                var response = httpClient.GetAsync("auctionproducts").Result;
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Received JSON from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));
                var products = System.Text.Json.JsonSerializer.Deserialize<List<AuctionProduct>>(json, serializerOptions);
                return products?.Cast<Product>().ToList() ?? new List<Product>();
            }
            catch (Exception getProductsException)
            {
                Console.WriteLine($"Error getting products: {getProductsException.Message}");
                if (getProductsException.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {getProductsException.InnerException.Message}");
                }
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
                return (Product)product;
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

        public Task<IEnumerable<Product>> SortAndFilter(string sortOption, string filterOption, string filterValue)
        {
            Console.WriteLine("Warning: SortAndFilter not implemented with specific API call yet. Returning all products.");
            return Task.FromResult(GetProducts().Cast<Product>());
        }
    }
}
