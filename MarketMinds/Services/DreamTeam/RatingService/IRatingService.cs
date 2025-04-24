using Marketplace_SE.Rating;

namespace Marketplace_SE.Services
{
    public interface IRatingService
    {
        void SaveRating(RatingData ratingData); // Synchronous method for saving a rating
    }
}
