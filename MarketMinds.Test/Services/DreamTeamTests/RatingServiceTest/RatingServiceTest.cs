using System;
using NUnit.Framework;
using Marketplace_SE.Rating;
using Marketplace_SE.Services;
using MarketMinds.Test.Services.DreamTeamTests.RatingServiceTest;

namespace MarketMinds.Test.Services.DreamTeamTests.RatingServiceTest
{
    [TestFixture]
    public class RatingServiceTest
    {
        private RatingService ratingService;
        private RatingRepositoryMock ratingRepositoryMock;

        private const string VALID_USER_ID = "user123";
        private const int VALID_RATING = 5;
        private const string VALID_COMMENT = "Excellent!";
        private const string VALID_APP_VERSION = "1.0.0";

        [SetUp]
        public void Setup()
        {
            ratingRepositoryMock = new RatingRepositoryMock();
            ratingService = new RatingService(ratingRepositoryMock);
        }

        [Test]
        public void TestSaveRating_CallsRepository()
        {
            var ratingData = new RatingData
            {
                UserID = VALID_USER_ID,
                Rating = VALID_RATING,
                Comment = VALID_COMMENT,
                Timestamp = DateTime.Now,
                AppVersion = VALID_APP_VERSION
            };

            ratingService.SaveRating(ratingData);

            Assert.That(ratingRepositoryMock.GetSaveCallCount(), Is.EqualTo(1));
            Assert.That(ratingRepositoryMock.LastSavedRating.UserID, Is.EqualTo(VALID_USER_ID));
        }
    }
}
