using Marketplace_SE.Rating;

namespace Marketplace_SE.Repositories
{
    public interface IRatingRepository
    {
        void SaveRating(RatingData ratingData); // Changed from Task to void
    }
}
