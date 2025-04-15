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
        private ReviewCalculationService _service;

        [SetUp]
        public void Setup()
        {
            _service = new ReviewCalculationService();
        }

        private Review CreateReview(int id, string text, List<Image> images, float rating, int authorId)
        {
            return new Review(id, text, images, rating, authorId, 1000);
        }

        [Test]
        public void CalculateAverageRating_WithValidReviews_ReturnsCorrectAverage()
        {
            var reviews = new List<Review>
            {
                CreateReview(1, "Good product", new List<Image>(), 4, 101),
                CreateReview(2, "Excellent", new List<Image>(), 5, 102),
                CreateReview(3, "Average", new List<Image>(), 3, 103)
            };

            float result = _service.CalculateAverageRating(reviews);

            Assert.That(result, Is.EqualTo(4.0f));
        }

        [Test]
        public void CalculateAverageRating_WithEmptyList_ReturnsZero()
        {
            var reviews = new List<Review>();
            float result = _service.CalculateAverageRating(reviews);

            Assert.That(result, Is.EqualTo(0f));
        }

        [Test]
        public void CalculateAverageRating_WithNullList_ReturnsZero()
        {
            List<Review> reviews = null;
            float result = _service.CalculateAverageRating(reviews);

            Assert.That(result, Is.EqualTo(0f));
        }

        [Test]
        public void CalculateAverageRating_WithAllZeroRatings_ReturnsZero()
        {
            var reviews = new List<Review>
            {
                CreateReview(1, "Poor", new List<Image>(), 0, 101),
                CreateReview(2, "Bad", new List<Image>(), 0, 102),
                CreateReview(3, "Terrible", new List<Image>(), 0, 103)
            };

            float result = _service.CalculateAverageRating(reviews);

            Assert.That(result, Is.EqualTo(0f));
        }

        [Test]
        public void CalculateAverageRating_WithNegativeRatings_ReturnsCorrectAverage()
        {
            var reviews = new List<Review>
            {
                CreateReview(1, "Awful", new List<Image>(), -1, 101),
                CreateReview(2, "Very Bad", new List<Image>(), -2, 102),
                CreateReview(3, "Horrible", new List<Image>(), -3, 103)
            };

            float result = _service.CalculateAverageRating(reviews);

            Assert.That(result, Is.EqualTo(-2.0f));
        }

        [Test]
        public void GetReviewCount_WithValidReviews_ReturnsCorrectCount()
        {
            var reviews = new List<Review>
            {
                CreateReview(1, "First review", new List<Image>(), 4, 101),
                CreateReview(2, "Second review", new List<Image>(), 5, 102),
                CreateReview(3, "Third review", new List<Image>(), 3, 103)
            };

            int result = _service.GetReviewCount(reviews);

            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetReviewCount_WithEmptyList_ReturnsZero()
        {
            var reviews = new List<Review>();
            int result = _service.GetReviewCount(reviews);

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetReviewCount_WithNullList_ReturnsZero()
        {
            List<Review> reviews = null;
            int result = _service.GetReviewCount(reviews);

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void AreReviewsEmpty_WithValidReviews_ReturnsFalse()
        {
            var reviews = new List<Review>
            {
                CreateReview(1, "First review", new List<Image>(), 4, 101),
                CreateReview(2, "Second review", new List<Image>(), 5, 102)
            };

            bool result = _service.AreReviewsEmpty(reviews);

            Assert.That(result, Is.False);
        }

        [Test]
        public void AreReviewsEmpty_WithEmptyList_ReturnsTrue()
        {
            var reviews = new List<Review>();
            bool result = _service.AreReviewsEmpty(reviews);

            Assert.That(result, Is.True);
        }

        [Test]
        public void AreReviewsEmpty_WithNullList_ReturnsTrue()
        {
            List<Review> reviews = null;
            bool result = _service.AreReviewsEmpty(reviews);

            Assert.That(result, Is.True);
        }
    }
}