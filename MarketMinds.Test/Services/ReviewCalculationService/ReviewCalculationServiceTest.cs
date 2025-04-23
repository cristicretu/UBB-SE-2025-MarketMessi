using DomainLayer.Domain;
using MarketMinds.Services;
using NUnit.Framework;
using System.Collections.Generic;
using MarketMinds.Services.ReviewCalculationService;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class ReviewCalculationServiceTest
    {
        // Constants for review properties
        private const int ReviewId1 = 1;
        private const int ReviewId2 = 2;
        private const int ReviewId3 = 3;

        // Constants for seller IDs
        private const int SellerId1 = 101;
        private const int SellerId2 = 102;
        private const int SellerId3 = 103;

        // Constants for buyer ID
        private const int BuyerId = 1000;

        // Constants for review texts
        private const string GoodProductText = "Good product";
        private const string ExcellentText = "Excellent";
        private const string AverageText = "Average";
        private const string PoorText = "Poor";
        private const string BadText = "Bad";
        private const string TerribleText = "Terrible";
        private const string AwfulText = "Awful";
        private const string VeryBadText = "Very Bad";
        private const string HorribleText = "Horrible";
        private const string FirstReviewText = "First review";
        private const string SecondReviewText = "Second review";
        private const string ThirdReviewText = "Third review";

        // Constants for ratings
        private const float FourStarRating = 4.0f;
        private const float FiveStarRating = 5.0f;
        private const float ThreeStarRating = 3.0f;
        private const float ZeroStarRating = 0.0f;
        private const float NegativeOneStarRating = -1.0f;
        private const float NegativeTwoStarRating = -2.0f;
        private const float NegativeThreeStarRating = -3.0f;

        // Constants for expected results
        private const float ExpectedAverageFourStars = 4.0f;
        private const float ExpectedAverageZero = 0.0f;
        private const float ExpectedAverageNegativeTwo = -2.0f;
        private const int ExpectedCountZero = 0;
        private const int ExpectedCountThree = 3;
        private const int ExpectedCountTwo = 2;

        private ReviewCalculationService _service;

        [SetUp]
        public void Setup()
        {
            _service = new ReviewCalculationService();
        }

        private Review CreateReview(int id, string text, List<Image> images, float rating, int authorId)
        {
            return new Review(id, text, images, rating, authorId, BuyerId);
        }

        [Test]
        public void CalculateAverageRating_WithValidReviews_ReturnsCorrectAverage()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(ReviewId1, GoodProductText, new List<Image>(), FourStarRating, SellerId1),
                CreateReview(ReviewId2, ExcellentText, new List<Image>(), FiveStarRating, SellerId2),
                CreateReview(ReviewId3, AverageText, new List<Image>(), ThreeStarRating, SellerId3)
            };

            // Act
            float result = _service.CalculateAverageRating(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(ExpectedAverageFourStars));
        }

        [Test]
        public void CalculateAverageRating_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var reviews = new List<Review>();

            // Act
            float result = _service.CalculateAverageRating(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(ExpectedAverageZero));
        }

        [Test]
        public void CalculateAverageRating_WithNullList_ReturnsZero()
        {
            // Arrange
            List<Review> reviews = null;

            // Act
            float result = _service.CalculateAverageRating(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(ExpectedAverageZero));
        }

        [Test]
        public void CalculateAverageRating_WithAllZeroRatings_ReturnsZero()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(ReviewId1, PoorText, new List<Image>(), ZeroStarRating, SellerId1),
                CreateReview(ReviewId2, BadText, new List<Image>(), ZeroStarRating, SellerId2),
                CreateReview(ReviewId3, TerribleText, new List<Image>(), ZeroStarRating, SellerId3)
            };

            // Act
            float result = _service.CalculateAverageRating(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(ExpectedAverageZero));
        }

        [Test]
        public void CalculateAverageRating_WithNegativeRatings_ReturnsCorrectAverage()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(ReviewId1, AwfulText, new List<Image>(), NegativeOneStarRating, SellerId1),
                CreateReview(ReviewId2, VeryBadText, new List<Image>(), NegativeTwoStarRating, SellerId2),
                CreateReview(ReviewId3, HorribleText, new List<Image>(), NegativeThreeStarRating, SellerId3)
            };

            // Act
            float result = _service.CalculateAverageRating(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(ExpectedAverageNegativeTwo));
        }

        [Test]
        public void GetReviewCount_WithValidReviews_ReturnsCorrectCount()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(ReviewId1, FirstReviewText, new List<Image>(), FourStarRating, SellerId1),
                CreateReview(ReviewId2, SecondReviewText, new List<Image>(), FiveStarRating, SellerId2),
                CreateReview(ReviewId3, ThirdReviewText, new List<Image>(), ThreeStarRating, SellerId3)
            };

            // Act
            int result = _service.GetReviewCount(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(ExpectedCountThree));
        }

        [Test]
        public void GetReviewCount_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var reviews = new List<Review>();

            // Act
            int result = _service.GetReviewCount(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(ExpectedCountZero));
        }

        [Test]
        public void GetReviewCount_WithNullList_ReturnsZero()
        {
            // Arrange
            List<Review> reviews = null;

            // Act
            int result = _service.GetReviewCount(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(ExpectedCountZero));
        }

        [Test]
        public void AreReviewsEmpty_WithValidReviews_ReturnsFalse()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(ReviewId1, FirstReviewText, new List<Image>(), FourStarRating, SellerId1),
                CreateReview(ReviewId2, SecondReviewText, new List<Image>(), FiveStarRating, SellerId2)
            };

            // Act
            bool result = _service.AreReviewsEmpty(reviews);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AreReviewsEmpty_WithEmptyList_ReturnsTrue()
        {
            // Arrange
            var reviews = new List<Review>();

            // Act
            bool result = _service.AreReviewsEmpty(reviews);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void AreReviewsEmpty_WithNullList_ReturnsTrue()
        {
            // Arrange
            List<Review> reviews = null;

            // Act
            bool result = _service.AreReviewsEmpty(reviews);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
