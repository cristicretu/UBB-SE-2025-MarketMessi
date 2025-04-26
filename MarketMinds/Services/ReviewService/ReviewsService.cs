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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public ObservableCollection<Review> GetReviewsBySeller(User seller)
        {
            if (seller == null)
            {
                return new ObservableCollection<Review>();
            }

            try
            {
                // Get the JSON response as a string first
                var response = httpClient.GetAsync($"review/seller/{seller.Id}").Result;

                if (!response.IsSuccessStatusCode)
                {
                    return new ObservableCollection<Review>();
                }

                var jsonString = response.Content.ReadAsStringAsync().Result;

                // Deserialize to list of client-side Review objects
                var reviews = JsonSerializer.Deserialize<List<Review>>(jsonString, jsonOptions);
                var result = new List<Review>();

                if (reviews != null)
                {
                    foreach (var review in reviews)
                    {
                        // Ensure rating is within the expected range (0-5)
                        // If rating is on a different scale, normalize it
                        if (review.Rating < 0)
                        {
                            review.Rating = 0;
                        }
                        else if (review.Rating > 5)
                        {
                            review.Rating = 5;
                        }

                        // Set username properties for UI display
                        review.SellerUsername = seller.Username;
                        review.BuyerUsername = $"User #{review.BuyerId}";

                        // Try to find test users that match the buyer ID
                        if (App.CurrentUser.Id == review.BuyerId)
                        {
                            review.BuyerUsername = App.CurrentUser.Username;
                        }
                        else if (App.TestingUser.Id == review.BuyerId)
                        {
                            review.BuyerUsername = App.TestingUser.Username;
                        }

                        result.Add(review);
                    }
                }

                // Convert to ObservableCollection
                return new ObservableCollection<Review>(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving seller reviews: {ex.Message}");
                return new ObservableCollection<Review>();
            }
        }

        public ObservableCollection<Review> GetReviewsByBuyer(User buyer)
        {
            if (buyer == null)
            {
                return new ObservableCollection<Review>();
            }

            try
            {
                // Get the JSON response as a string first
                var response = httpClient.GetAsync($"review/buyer/{buyer.Id}").Result;

                if (!response.IsSuccessStatusCode)
                {
                    return new ObservableCollection<Review>();
                }

                var jsonString = response.Content.ReadAsStringAsync().Result;

                // Deserialize to list of client-side Review objects
                var reviews = JsonSerializer.Deserialize<List<Review>>(jsonString, jsonOptions);
                var result = new List<Review>();

                if (reviews != null)
                {
                    foreach (var review in reviews)
                    {
                        // Ensure rating is within the expected range (0-5)
                        // If rating is on a different scale, normalize it
                        if (review.Rating < 0)
                        {
                            review.Rating = 0;
                        }
                        else if (review.Rating > 5)
                        {
                            review.Rating = 5;
                        }

                        // Set username properties for UI display
                        review.BuyerUsername = buyer.Username;
                        review.SellerUsername = $"User #{review.SellerId}";

                        // Try to find test users that match the seller ID
                        if (App.CurrentUser.Id == review.SellerId)
                        {
                            review.SellerUsername = App.CurrentUser.Username;
                        }
                        else if (App.TestingUser.Id == review.SellerId)
                        {
                            review.SellerUsername = App.TestingUser.Username;
                        }

                        result.Add(review);
                    }
                }

                // Convert to ObservableCollection
                return new ObservableCollection<Review>(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving buyer reviews: {ex.Message}");
                return new ObservableCollection<Review>();
            }
        }

        public void AddReview(string description, List<Image> images, double rating, User seller, User buyer)
        {

            // Ensure rating is within the expected range (0-5)
            double validRating = Math.Max(0, Math.Min(5, rating));

            // Create a Review object directly
            var review = new Review
            {
                Description = description,
                Images = images ?? new List<Image>(),
                Rating = validRating,
                SellerId = seller.Id,
                BuyerId = buyer.Id
            };

            var response = httpClient.PostAsJsonAsync("review", review, jsonOptions).Result;
            response.EnsureSuccessStatusCode();
        }

        public void EditReview(string description, List<Image> images, double rating, int sellerid, int buyerid, string newDescription, double newRating)
        {
            // Ensure new rating is within the expected range (0-5)
            double validRating = Math.Max(0, Math.Min(5, newRating));

            // Create a Review object with the updated values
            var updatedReview = new Review
            {
                Description = newDescription,
                Images = images ?? new List<Image>(),
                Rating = validRating,
                SellerId = sellerid,
                BuyerId = buyerid
            };

            var response = httpClient.PutAsJsonAsync("review", updatedReview, jsonOptions).Result;
            response.EnsureSuccessStatusCode();
        }

        public void DeleteReview(string description, List<Image> images, double rating, int sellerid, int buyerid)
        {
            // Ensure rating is within the expected range (0-5)
            double validRating = Math.Max(0, Math.Min(5, rating));

            // Create a Review object to delete
            var reviewToDelete = new Review
            {
                Description = description,
                Images = images ?? new List<Image>(),
                Rating = validRating,
                SellerId = sellerid,
                BuyerId = buyerid
            };

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
