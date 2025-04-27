using Marketplace_SE.Rating;
using Marketplace_SE.Repositories;

namespace MarketMinds.Test.Services.DreamTeamTests.RatingServiceTest
{
    public class RatingRepositoryMock : IRatingRepository
    {
        private int saveCallCount = 0;
        public RatingData LastSavedRating { get; private set; }

        public void SaveRating(RatingData ratingData)
        {
            saveCallCount++;
            LastSavedRating = ratingData;
        }

        // Helper to check how many times Save was called
        public int GetSaveCallCount()
        {
            return saveCallCount;
        }
    }
}
