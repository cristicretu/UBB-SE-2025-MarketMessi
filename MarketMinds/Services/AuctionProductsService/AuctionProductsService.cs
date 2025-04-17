using System.Threading.Tasks;
using System.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using DomainLayer.Domain;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MarketMinds.Services.ProductTagService;

namespace MarketMinds.Services.AuctionProductsService
{
    public class AuctionProductsService : ProductService, IAuctionProductsService
    {
        private const int NULL_BID_AMOUNT = 0;
        private const int MAX_AUCTION_TIME = 5;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public AuctionProductsService(IConfiguration configuration) : base(null)
        {
            _httpClient = new HttpClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!_apiBaseUrl.EndsWith("/")) _apiBaseUrl += "/";
            _httpClient.BaseAddress = new Uri(_apiBaseUrl + "api/");
        }

        public void CreateListing(Product product)
        {
            if (!(product is AuctionProduct auctionProduct))
            {
                throw new ArgumentException("Product must be an AuctionProduct.", nameof(product));
            }
            var response = _httpClient.PostAsJsonAsync("auctionproducts", auctionProduct).Result;
            response.EnsureSuccessStatusCode();
        }

        public void PlaceBid(AuctionProduct auction, User bidder, float bidAmount)
        {
            ValidateBid(auction, bidder, bidAmount);

            bidder.Balance -= bidAmount;

            RefundPreviousBidder(auction);

            var bid = new Bid(bidder, bidAmount, DateTime.Now);
            auction.AddBid(bid);
            auction.CurrentPrice = bidAmount;

            ExtendAuctionTime(auction);

            var response = _httpClient.PutAsJsonAsync($"auctionproducts/{auction.Id}", auction).Result;
            response.EnsureSuccessStatusCode();
        }

        public void ConcludeAuction(AuctionProduct auction)
        {
            if (auction.Id == 0)
            {
                throw new ArgumentException("Auction Product ID must be set for delete.", nameof(auction.Id));
            }
            var response = _httpClient.DeleteAsync($"auctionproducts/{auction.Id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public string GetTimeLeft(AuctionProduct auction)
        {
            var timeLeft = auction.EndAuctionDate - DateTime.Now;
            return timeLeft > TimeSpan.Zero ? timeLeft.ToString(@"dd\:hh\:mm\:ss") : "Auction Ended";
        }

        public bool IsAuctionEnded(AuctionProduct auction)
        {
            return DateTime.Now >= auction.EndAuctionDate;
        }

        private void ValidateBid(AuctionProduct auction, User bidder, float bidAmount)
        {
            float minBid = auction.BidHistory.Count == NULL_BID_AMOUNT ? auction.StartingPrice : auction.CurrentPrice + 1;

            if (bidAmount < minBid)
            {
                throw new Exception($"Bid must be at least ${minBid}");
            }

            if (bidAmount > bidder.Balance)
            {
                throw new Exception("Insufficient balance");
            }

            if (DateTime.Now > auction.EndAuctionDate)
            {
                throw new Exception("Auction already ended");
            }
        }

        private void RefundPreviousBidder(AuctionProduct auction)
        {
            if (auction.BidHistory.Count > 0)
            {
                var previousBid = auction.BidHistory.Last();
                Console.WriteLine($"Warning: Client-side refund logic for bidder ID {previousBid.Bidder.Id} needs review.");
            }
        }

        private void ExtendAuctionTime(AuctionProduct auction)
        {
            var timeRemaining = auction.EndAuctionDate - DateTime.Now;

            if (timeRemaining.TotalMinutes < MAX_AUCTION_TIME)
            {
                auction.EndAuctionDate = DateTime.Now.AddMinutes(MAX_AUCTION_TIME);
            }
        }

        public override List<Product> GetProducts()
        {
            var products = _httpClient.GetFromJsonAsync<List<AuctionProduct>>("auctionproducts").Result;
            return products?.Cast<Product>().ToList() ?? new List<Product>();
        }

        public override Product GetProductById(int id)
        {
            var product = _httpClient.GetFromJsonAsync<AuctionProduct>($"auctionproducts/{id}").Result;
            if (product == null)
            {
                throw new KeyNotFoundException($"Auction product with ID {id} not found.");
            }
            return product;
        }

        public Task<IEnumerable<Product>> SortAndFilter(string sortOption, string filterOption, string filterValue)
        {
            Console.WriteLine("Warning: SortAndFilter not implemented with specific API call yet. Returning all products.");
            return Task.FromResult(GetProducts().Cast<Product>());
        }
    }
}
