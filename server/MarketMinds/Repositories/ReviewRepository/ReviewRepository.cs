using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using server.Models;
using Microsoft.EntityFrameworkCore;
using server.DataAccessLayer;

namespace MarketMinds.Repositories.ReviewRepository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public ObservableCollection<Review> GetAllReviewsByBuyer(User buyer)
        {
            var reviews = _context.Reviews
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
            var reviews = _context.Reviews
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

            // Add the review to the context
            _context.Reviews.Add(review);
            _context.SaveChanges();

            // Now that we have an ID, we can sync the images
            // We need to restore the Images collection first since it might have been cleared by EF
            review.Images = originalImages;

            // Create ReviewImages from the original Images
            if (review.Images != null && review.Images.Count > 0)
            {
                foreach (var image in review.Images)
                {
                    var reviewImage = ReviewImage.FromImage(image, review.Id);
                    _context.ReviewImages.Add(reviewImage);
                }
                _context.SaveChanges();
            }

            // Load the review images into the Images collection
            review.LoadGenericImages();
        }

        public void EditReview(Review review, double rating, string description)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            // Find the review in the database
            var existingReview = _context.Reviews
                .Include(r => r.ReviewImages)
                .FirstOrDefault(r => r.Id == review.Id);

            if (existingReview == null)
            {
                // If not found by ID, try to find by buyer, seller and description
                existingReview = _context.Reviews
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
                _context.ReviewImages.RemoveRange(existingReview.ReviewImages);

                // Add new images
                foreach (var image in review.Images)
                {
                    var reviewImage = ReviewImage.FromImage(image, existingReview.Id);
                    _context.ReviewImages.Add(reviewImage);
                }
            }

            _context.SaveChanges();
        }

        public void DeleteReview(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            // Find the review in the database
            var existingReview = _context.Reviews
                .Include(r => r.ReviewImages)
                .FirstOrDefault(r => r.Id == review.Id);

            if (existingReview == null)
            {
                // If not found by ID, try to find by buyer, seller and description
                existingReview = _context.Reviews
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
            _context.ReviewImages.RemoveRange(existingReview.ReviewImages);

            // Then remove the review
            _context.Reviews.Remove(existingReview);

            _context.SaveChanges();
        }
    }
}
