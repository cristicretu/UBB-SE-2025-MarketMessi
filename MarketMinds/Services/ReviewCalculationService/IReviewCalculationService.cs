using System.Collections.Generic;
using DomainLayer.Domain;

namespace MarketMinds.Services.ReviewCalculationService
{
    /// <summary>
    /// Interface for ReviewCalculationService to manage review-related calculations.
    /// </summary>
    public interface IReviewCalculationService
    {
        /// <summary>
        /// Calculates the average rating from a collection of reviews.
        /// </summary>
        /// <param name="reviews">The collection of reviews.</param>
        /// <returns>The average rating, or 0 if there are no reviews.</returns>
        float CalculateAverageRating(IEnumerable<Review> reviews);

        /// <summary>
        /// Gets the total count of reviews in a collection.
        /// </summary>
        /// <param name="reviews">The collection of reviews.</param>
        /// <returns>The count of reviews, or 0 if the collection is null.</returns>
        int GetReviewCount(IEnumerable<Review> reviews);

        /// <summary>
        /// Checks if a collection of reviews is empty or null.
        /// </summary>
        /// <param name="reviews">The collection of reviews.</param>
        /// <returns>True if the collection is null or empty, otherwise false.</returns>
        bool AreReviewsEmpty(IEnumerable<Review> reviews);
    }
}

