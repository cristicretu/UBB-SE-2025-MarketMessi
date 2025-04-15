using System.Collections.Generic;
using System.Linq;
using DomainLayer.Domain;

namespace MarketMinds.Services.ReviewCalculationService
{
    public class ReviewCalculationService : IReviewCalculationService
    {
        private const int NO_REVIEWS = 0;
        private const int NO_RATINGS = 0;

        public float CalculateAverageRating(IEnumerable<Review> reviews)
        {
            if (reviews == null || !reviews.Any())
            {
                return NO_RATINGS;
            }
            return reviews.Average(r => r.Rating);
        }

        public int GetReviewCount(IEnumerable<Review> reviews)
        {
            return reviews?.Count() ?? NO_REVIEWS;
        }

        public bool AreReviewsEmpty(IEnumerable<Review> reviews)
        {
            return reviews == null || !reviews.Any();
        }
    }
}