using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.ReviewService
{
    public class ReviewsService : IReviewsService
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;
        private readonly JsonSerializerOptions jsonOptions;

        public ReviewsService(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");

            // Configure JSON options for proper deserialization
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.Preserve // Enable reference handling
            };
        }

        public ObservableCollection<Review> GetReviewsBySeller(User seller)
        {
            try
            {
                // Get the JSON response as a string first
                var response = httpClient.GetAsync($"review/seller/{seller.Id}").Result;

                if (!response.IsSuccessStatusCode)
                {
                    return new ObservableCollection<Review>();
                }

                var jsonString = response.Content.ReadAsStringAsync().Result;

                // Then deserialize using System.Text.Json which handles the reference format
                var reviews = JsonSerializer.Deserialize<List<Review>>(jsonString, jsonOptions);

                // Convert to ObservableCollection
                return reviews != null
                    ? new ObservableCollection<Review>(reviews)
                    : new ObservableCollection<Review>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving seller reviews: {ex.Message}");
                return new ObservableCollection<Review>();
            }
        }

        public ObservableCollection<Review> GetReviewsByBuyer(User buyer)
        {
            try
            {
                // Get the JSON response as a string first
                var response = httpClient.GetAsync($"review/buyer/{buyer.Id}").Result;

                if (!response.IsSuccessStatusCode)
                {
                    return new ObservableCollection<Review>();
                }

                var jsonString = response.Content.ReadAsStringAsync().Result;

                // Then deserialize using System.Text.Json which handles the reference format
                var reviews = JsonSerializer.Deserialize<List<Review>>(jsonString, jsonOptions);

                // Convert to ObservableCollection
                return reviews != null
                    ? new ObservableCollection<Review>(reviews)
                    : new ObservableCollection<Review>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving buyer reviews: {ex.Message}");
                return new ObservableCollection<Review>();
            }
        }

        public void AddReview(string description, List<Image> images, double rating, User seller, User buyer)
        {
            // Create the review object without an ID
            var reviewDto = new
            {
                Description = description,
                Images = images,
                Rating = rating,
                SellerId = seller.Id,
                BuyerId = buyer.Id
            };

            var response = httpClient.PostAsJsonAsync("review", reviewDto, jsonOptions).Result;
            response.EnsureSuccessStatusCode();
        }

        public void EditReview(string description, List<Image> images, double rating, int sellerid, int buyerid, string newDescription, double newRating)
        {
            // Create a review object with the updated values
            var updatedReview = new Review(
                -1, // The API will find the review based on other fields
                description,
                images,
                rating,
                sellerid,
                buyerid);

            // Update with new values
            updatedReview.Description = newDescription;
            updatedReview.Rating = newRating;

            var response = httpClient.PutAsJsonAsync("review", updatedReview, jsonOptions).Result;
            response.EnsureSuccessStatusCode();
        }

        public void DeleteReview(string description, List<Image> images, double rating, int sellerid, int buyerid)
        {
            // Create a review object to delete
            var reviewToDelete = new Review(
                -1, // The API will find the review based on other fields
                description,
                images,
                rating,
                sellerid,
                buyerid);

            // Using HttpRequestMessage for DELETE with body
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(httpClient.BaseAddress + "review"),
                Content = JsonContent.Create(reviewToDelete, options: jsonOptions)
            };

            var response = httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
        }
    }
}
