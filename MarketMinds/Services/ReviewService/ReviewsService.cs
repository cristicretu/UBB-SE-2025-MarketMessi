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
using MarketMinds.Services.UserService;

namespace MarketMinds.Services.ReviewService
{
    public class ReviewsService : IReviewsService
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;
        private readonly JsonSerializerOptions jsonOptions;
        private Dictionary<int, string> userCache = new Dictionary<int, string>();
        private readonly IUserService userService;
        private readonly IConfiguration configuration;

        public ReviewsService(IConfiguration configuration)
        {
            this.configuration = configuration;
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

        private async Task<string> GetUsernameForIdAsync(int userId)
        {
            if (App.CurrentUser != null && App.CurrentUser.Id == userId)
            {
                return App.CurrentUser.Username;
            }

            if (userCache.ContainsKey(userId))
            {
                return userCache[userId];
            }

            try
            {
                {
                    try
                    {
                        var user = await App.UserService.GetUserByIdAsync(userId);

                        if (user != null)
                        {
                            userCache[userId] = user.Username;
                            return user.Username;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error using UserService: {ex.Message}. Falling back to direct API call.");
                    }
                }

                var userResponse = httpClient.GetAsync($"users/id/{userId}").Result;

                if (userResponse.IsSuccessStatusCode)
                {
                    var userJsonString = userResponse.Content.ReadAsStringAsync().Result;

                    var userDto = JsonSerializer.Deserialize<JsonElement>(userJsonString, jsonOptions);

                    if (userDto.TryGetProperty("username", out JsonElement usernameElement) &&
                        usernameElement.ValueKind == JsonValueKind.String)
                    {
                        string username = usernameElement.GetString();
                        userCache[userId] = username;
                        return username;
                    }
                }

                string fallbackUsername = $"User #{userId}";
                userCache[userId] = fallbackUsername;
                return fallbackUsername;
            }
            catch (Exception ex)
            {
                string fallbackUsername = $"User #{userId}";
                userCache[userId] = fallbackUsername;
                return fallbackUsername;
            }
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
                    var usernameTasks = new List<Task>();

                    foreach (var review in reviews)
                    {
                        // Ensure rating is within the expected range (0-5)
                        if (review.Rating < 0)
                        {
                            review.Rating = 0;
                        }
                        else if (review.Rating > 5)
                        {
                            review.Rating = 5;
                        }

                        review.SellerUsername = seller.Username;

                        var fetchUsernameTask = Task.Run(async () =>
                        {
                            review.BuyerUsername = await GetUsernameForIdAsync(review.BuyerId);
                        });

                        usernameTasks.Add(fetchUsernameTask);
                        result.Add(review);
                    }

                    Task.WhenAll(usernameTasks).Wait();
                }

                return new ObservableCollection<Review>(result);
            }
            catch (Exception ex)
            {
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
                    var usernameTasks = new List<Task>();

                    foreach (var review in reviews)
                    {
                        // Ensure rating is within the expected range (0-5)
                        if (review.Rating < 0)
                        {
                             review.Rating = 0;
                        }
                        else if (review.Rating > 5)
                        {
                            review.Rating = 5;
                        }

                        review.BuyerUsername = buyer.Username;

                        var fetchUsernameTask = Task.Run(async () =>
                        {
                            review.SellerUsername = await GetUsernameForIdAsync(review.SellerId);
                        });

                        usernameTasks.Add(fetchUsernameTask);
                        result.Add(review);
                    }

                    Task.WhenAll(usernameTasks).Wait();
                }

                return new ObservableCollection<Review>(result);
            }
            catch (Exception ex)
            {
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

            userCache.Clear();
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

            userCache.Clear();
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

            userCache.Clear();
        }
    }
}
