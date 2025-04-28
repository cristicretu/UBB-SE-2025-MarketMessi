using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Server.Models;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;

namespace MarketMinds.Repositories.ReviewRepository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext context;

        public ReviewRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public ObservableCollection<Review> GetAllReviewsByBuyer(User buyer)
        {
            var reviews = context.Reviews
                .Include(r => r.ReviewImages)
                .Where(r => r.BuyerId == buyer.Id)
                .ToList();

            // Convert to ObservableCollection
            var observableReviews = new ObservableCollection<Review>();
            foreach (var review in reviews)
            {
                review.LoadGenericImages();
                observableReviews.Add(review);
            }

            return observableReviews;
        }

        public ObservableCollection<Review> GetAllReviewsBySeller(User seller)
        {
            var reviews = context.Reviews
                .Include(r => r.ReviewImages)
                .Where(r => r.SellerId == seller.Id)
                .ToList();

            // Convert to ObservableCollection
            var observableReviews = new ObservableCollection<Review>();
            foreach (var review in reviews)
            {
                review.LoadGenericImages();
                observableReviews.Add(review);
            }

            return observableReviews;
        }

        public void CreateReview(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            // Store the original Images collection
            var originalImages = new List<Image>(review.Images);

            // Create a new review object without setting the ID - let the database generate it
            var newReview = new Review
            {
                Description = review.Description ?? string.Empty,
                Rating = review.Rating,
                SellerId = review.SellerId,
                BuyerId = review.BuyerId
            };

            // Add the review to the context
            context.Reviews.Add(newReview);
            context.SaveChanges();

            // Now that we have an ID, we can sync the images
            // We need to restore the Images collection first since it might have been cleared by EF
            if (originalImages != null && originalImages.Count > 0)
            {
                foreach (var image in originalImages)
                {
                    var reviewImage = ReviewImage.FromImage(image, newReview.Id);
                    context.ReviewImages.Add(reviewImage);
                }
                context.SaveChanges();
            }

            // Update the original review object with the generated ID
            review.Id = newReview.Id;

            // Load the review images into the Images collection
            newReview.LoadGenericImages();
            review.Images = new List<Image>(newReview.Images);
        }

        public void EditReview(Review review, double rating, string description)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            // Find the review in the database
            var existingReview = context.Reviews
                .Include(r => r.ReviewImages)
                .FirstOrDefault(r => r.Id == review.Id);

            if (existingReview == null)
            {
                // If not found by ID, try to find by buyer, seller and description
                existingReview = context.Reviews
                    .Include(r => r.ReviewImages)
                    .FirstOrDefault(r =>
                        r.BuyerId == review.BuyerId &&
                        r.SellerId == review.SellerId &&
                        r.Description == review.Description);

                if (existingReview == null)
                {
                    throw new KeyNotFoundException($"Review not found with ID {review.Id}");
                }
            }

            // Update fields
            existingReview.Rating = rating;
            existingReview.Description = description;

            // Update images if provided
            if (review.Images != null && review.Images.Count > 0)
            {
                // Remove existing images
                context.ReviewImages.RemoveRange(existingReview.ReviewImages);

                // Add new images
                foreach (var image in review.Images)
                {
                    var reviewImage = ReviewImage.FromImage(image, existingReview.Id);
                    context.ReviewImages.Add(reviewImage);
                }
            }

            context.SaveChanges();
        }

        public void DeleteReview(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            // Find the review in the database
            var existingReview = context.Reviews
                .Include(r => r.ReviewImages)
                .FirstOrDefault(r => r.Id == review.Id);

            if (existingReview == null)
            {
                // If not found by ID, try to find by buyer, seller and description
                existingReview = context.Reviews
                    .Include(r => r.ReviewImages)
                    .FirstOrDefault(r =>
                        r.BuyerId == review.BuyerId &&
                        r.SellerId == review.SellerId &&
                        r.Description == review.Description);

                if (existingReview == null)
                {
                    throw new KeyNotFoundException($"Review not found with ID {review.Id}");
                }
            }

            // Remove related images first
            context.ReviewImages.RemoveRange(existingReview.ReviewImages);

            // Then remove the review
            context.Reviews.Remove(existingReview);

            context.SaveChanges();
        }
    }
}
