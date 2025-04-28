using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;
        private Dictionary<int, string> userCache = new Dictionary<int, string>();

        public ReviewRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }

            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }

            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
            Console.WriteLine($"Review Repository initialized with base address: {httpClient.BaseAddress}");

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public ObservableCollection<Review> GetAllReviewsBySeller(User seller)
        {
            if (seller == null)
            {
                return new ObservableCollection<Review>();
            }

            try
            {
                var response = httpClient.GetAsync($"review/seller/{seller.Id}").Result;

                if (!response.IsSuccessStatusCode)
                {
                    return new ObservableCollection<Review>();
                }

                var jsonString = response.Content.ReadAsStringAsync().Result;
                var reviews = JsonSerializer.Deserialize<List<Review>>(jsonString, jsonOptions);
                
                return reviews != null ? new ObservableCollection<Review>(reviews) : new ObservableCollection<Review>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reviews by seller: {ex.Message}");
                return new ObservableCollection<Review>();
            }
        }

        public ObservableCollection<Review> GetAllReviewsByBuyer(User buyer)
        {
            if (buyer == null)
            {
                return new ObservableCollection<Review>();
            }

            try
            {
                var response = httpClient.GetAsync($"review/buyer/{buyer.Id}").Result;

                if (!response.IsSuccessStatusCode)
                {
                    return new ObservableCollection<Review>();
                }

                var jsonString = response.Content.ReadAsStringAsync().Result;
                var reviews = JsonSerializer.Deserialize<List<Review>>(jsonString, jsonOptions);
                
                return reviews != null ? new ObservableCollection<Review>(reviews) : new ObservableCollection<Review>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reviews by buyer: {ex.Message}");
                return new ObservableCollection<Review>();
            }
        }

        public void CreateReview(Review review)
        {
            try
            {
                var response = httpClient.PostAsJsonAsync("review", review, jsonOptions).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating review: {ex.Message}");
                throw;
            }
        }

        public void EditReview(Review review, double rating, string description)
        {
            try
            {
                var updateRequest = new
                {
                    ReviewId = review.Id,
                    SellerId = review.SellerId,
                    BuyerId = review.BuyerId,
                    Description = description,
                    Rating = rating
                };

                var response = httpClient.PutAsJsonAsync("review", updateRequest, jsonOptions).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing review: {ex.Message}");
                throw;
            }
        }

        public void DeleteReview(Review review)
        {
            try
            {
                var deleteRequest = new
                {
                    ReviewId = review.Id,
                    SellerId = review.SellerId,
                    BuyerId = review.BuyerId
                };

                var requestContent = new StringContent(
                    JsonSerializer.Serialize(deleteRequest, jsonOptions),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(httpClient.BaseAddress + "review"),
                    Content = requestContent
                };

                var response = httpClient.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting review: {ex.Message}");
                throw;
            }
        }
    }
} 