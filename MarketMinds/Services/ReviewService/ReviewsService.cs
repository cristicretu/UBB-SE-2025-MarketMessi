using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.ReviewService
{
    public class ReviewsService : IReviewsService
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public ReviewsService(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public ObservableCollection<Review> GetReviewsBySeller(User seller)
        {
            var reviews = httpClient.GetFromJsonAsync<ObservableCollection<Review>>($"review/seller/{seller.Id}").Result;
            return reviews ?? new ObservableCollection<Review>();
        }

        public ObservableCollection<Review> GetReviewsByBuyer(User buyer)
        {
            var reviews = httpClient.GetFromJsonAsync<ObservableCollection<Review>>($"review/buyer/{buyer.Id}").Result;
            return reviews ?? new ObservableCollection<Review>();
        }

        public void AddReview(string description, List<Image> images, float rating, User seller, User buyer)
        {
            var review = new Review(
                -1, // ID will be assigned by the API
                description,
                images,
                rating,
                seller.Id,
                buyer.Id);

            var response = httpClient.PostAsJsonAsync("review", review).Result;
            response.EnsureSuccessStatusCode();
        }

        public void EditReview(string description, List<Image> images, float rating, int sellerid, int buyerid, string newDescription, float newRating)
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

            var response = httpClient.PutAsJsonAsync("review", updatedReview).Result;
            response.EnsureSuccessStatusCode();
        }

        public void DeleteReview(string description, List<Image> images, float rating, int sellerid, int buyerid)
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
                Content = JsonContent.Create(reviewToDelete)
            };

            var response = httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
        }
    }
}
