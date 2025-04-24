using Marketplace_SE.Rating;
using Marketplace_SE.Repositories;

namespace Marketplace_SE.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository ratingRepository;

        public RatingService(IRatingRepository ratingRepository)
        {
            this.ratingRepository = ratingRepository;
        }

        public void SaveRating(RatingData ratingData)
        {
            // Add any additional business logic here if needed
            ratingRepository.SaveRating(ratingData);
        }
    }
}
