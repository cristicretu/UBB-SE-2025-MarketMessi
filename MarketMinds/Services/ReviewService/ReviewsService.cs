using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Services.UserService;
using MarketMinds.ServiceProxy;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.ReviewService
{
    public class ReviewsService : IReviewsService
    {
        private readonly ReviewServiceProxy repository;
        private readonly IUserService userService;
        private Dictionary<int, string> userCache = new Dictionary<int, string>();

        public ReviewsService(IConfiguration configuration)
        {
            repository = new ReviewServiceProxy(configuration);
            // We'll need the user service to get usernames
            userService = App.UserService;

            // If userService is null, create a new instance with the configuration
            if (userService == null)
            {
                userService = new UserService.UserService(configuration);
            }
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
                // Ensure userService is not null before calling it
                if (userService == null)
                {
                    string fallbackUsernameLocal = $"User #{userId}"; // Renamed to avoid conflict
                    userCache[userId] = fallbackUsernameLocal;
                    return fallbackUsernameLocal;
                }

                var user = await userService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    userCache[userId] = user.Username;
                    return user.Username;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting username for ID {userId}: {ex.Message}");
            }

            string fallbackUsername = $"User #{userId}";
            userCache[userId] = fallbackUsername;
            return fallbackUsername;
        }

        public ObservableCollection<Review> GetReviewsBySeller(User seller)
        {
            if (seller == null)
            {
                return new ObservableCollection<Review>();
            }

            try
            {
                var sharedUser = ConvertToSharedUser(seller);
                var sharedReviews = repository.GetAllReviewsBySeller(sharedUser);
                var domainReviews = new List<Review>();

                foreach (var sharedReview in sharedReviews)
                {
                    var domainReview = ConvertToDomainReview(sharedReview);
                    domainReview.SellerUsername = seller.Username;
                    // Fetch buyer username asynchronously
                    Task.Run(async () =>
                    {
                        domainReview.BuyerUsername = await GetUsernameForIdAsync(domainReview.BuyerId);
                    }).Wait();
                    domainReviews.Add(domainReview);
                }

                return new ObservableCollection<Review>(domainReviews);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting reviews by seller: {ex.Message}");
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
                var sharedUser = ConvertToSharedUser(buyer);
                var sharedReviews = repository.GetAllReviewsByBuyer(sharedUser);
                var domainReviews = new List<Review>();

                foreach (var sharedReview in sharedReviews)
                {
                    var domainReview = ConvertToDomainReview(sharedReview);
                    domainReview.BuyerUsername = buyer.Username;
                    // Fetch seller username asynchronously
                    Task.Run(async () =>
                    {
                        domainReview.SellerUsername = await GetUsernameForIdAsync(domainReview.SellerId);
                    }).Wait();
                    domainReviews.Add(domainReview);
                }

                return new ObservableCollection<Review>(domainReviews);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting reviews by buyer: {ex.Message}");
                return new ObservableCollection<Review>();
            }
        }

        public void AddReview(string description, List<Image> images, double rating, User seller, User buyer)
        {
            try
            {
                // Ensure rating is within the expected range (0-5)
                double validRating = Math.Max(0, Math.Min(5, rating));

                // Create a shared Review object
                var sharedReview = new MarketMinds.Shared.Models.Review
                {
                    Description = description,
                    Images = ConvertToSharedImages(images ?? new List<Image>()),
                    Rating = validRating,
                    SellerId = seller.Id,
                    BuyerId = buyer.Id
                };

                repository.CreateReview(sharedReview);
                userCache.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding review: {ex.Message}");
                throw;
            }
        }

        public void EditReview(string description, List<Image> images, double rating, int sellerid, int buyerid, string newDescription, double newRating)
        {
            try
            {
                // Ensure the new rating is within the expected range (0-5)
                double validRating = Math.Max(0, Math.Min(5, newRating));

                // Create a shared Review object
                var sharedReview = new MarketMinds.Shared.Models.Review
                {
                    Description = description,
                    Images = ConvertToSharedImages(images ?? new List<Image>()),
                    Rating = rating,
                    SellerId = sellerid,
                    BuyerId = buyerid
                };

                repository.EditReview(sharedReview, validRating, newDescription);
                userCache.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error editing review: {ex.Message}");
                throw;
            }
        }

        public void DeleteReview(string description, List<Image> images, double rating, int sellerid, int buyerid)
        {
            try
            {
                // Create a shared Review object
                var sharedReview = new MarketMinds.Shared.Models.Review
                {
                    Description = description,
                    Images = ConvertToSharedImages(images ?? new List<Image>()),
                    Rating = rating,
                    SellerId = sellerid,
                    BuyerId = buyerid
                };

                repository.DeleteReview(sharedReview);
                userCache.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting review: {ex.Message}");
                throw;
            }
        }

        // Helper methods to convert between domain and shared models
        private MarketMinds.Shared.Models.User ConvertToSharedUser(User domainUser)
        {
            if (domainUser == null)
            {
                return null;
            }

            return new MarketMinds.Shared.Models.User
            {
                Id = domainUser.Id,
                Username = domainUser.Username,
                Email = domainUser.Email,
                PasswordHash = domainUser.PasswordHash,
                UserType = domainUser.UserType,
                Balance = domainUser.Balance,
                Rating = domainUser.Rating
            };
        }

        private Review ConvertToDomainReview(MarketMinds.Shared.Models.Review sharedReview)
        {
            if (sharedReview == null)
            {
                return null;
            }

            return new Review
            {
                Id = sharedReview.Id,
                Description = sharedReview.Description,
                Images = ConvertToDomainImages(sharedReview.Images),
                Rating = sharedReview.Rating,
                SellerId = sharedReview.SellerId,
                BuyerId = sharedReview.BuyerId,
                SellerUsername = string.Empty,  // Will be populated later
                BuyerUsername = string.Empty // Will be populated later
            };
        }

        private List<Image> ConvertToDomainImages(List<MarketMinds.Shared.Models.Image> sharedImages)
        {
            return sharedImages?.Select(img => new Image
            {
                Id = img.Id,
                Url = img.Url
            }).ToList() ?? new List<Image>();
        }

        private List<MarketMinds.Shared.Models.Image> ConvertToSharedImages(List<Image> domainImages)
        {
            return domainImages?.Select(img => new MarketMinds.Shared.Models.Image
            {
                Id = img.Id,
                Url = img.Url
            }).ToList() ?? new List<MarketMinds.Shared.Models.Image>();
        }
    }
}
