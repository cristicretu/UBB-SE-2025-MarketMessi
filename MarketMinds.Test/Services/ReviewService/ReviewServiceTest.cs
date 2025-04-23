using DomainLayer.Domain;
using MarketMinds.Services.ReviewService;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMinds.Test.Services.ReviewService
{
    [TestFixture]
    internal class ReviewServiceTest
    {
        // Constants for User IDs
        private const int Seller1Id = 1;
        private const int Seller2Id = 2;
        private const int BuyerId = 3;
        private const int Buyer1Id = 2;
        private const int Buyer2Id = 3;

        // Constants for User names
        private const string Seller1Name = "Marcel";
        private const string Seller2Name = "Dorel";
        private const string BuyerName = "Sorin";
        private const string SellerLucaName = "Luca";
        private const string BuyerCristiName = "Cristi";

        // Constants for email addresses
        private const string Seller1Email = "marcel@mail.com";
        private const string Seller2Email = "dorel@mail.com";
        private const string BuyerEmail = "sorin@mail.com";
        private const string SellerLucaEmail = "luca@mail.com";
        private const string BuyerCristiEmail = "cristi@mail.com";

        // Constants for Review IDs
        private const int Review1Id = 1;
        private const int Review2Id = 2;
        private const int Review3Id = 3;

        // Constants for Review descriptions
        private const string Review1Description = "Review 1";
        private const string Review2Description = "Review 2";
        private const string Review3Description = "Review 3";
        private const string LoveTestingDescription = "I love testing.";
        private const string LoveTestingNotDescription = "I love testing not.";

        // Constants for Review ratings
        private const float Rating4_5 = 4.5f;
        private const float Rating5_0 = 5.0f;
        private const float Rating3_5 = 3.5f;
        private const float Rating4_8 = 4.8f;
        private const float Rating3_0 = 3.0f;
        private const float Rating4_0 = 4.0f;

        // Constants for expected counts
        private const int ExpectedSingleItem = 1;
        private const int ExpectedTwoItems = 2;
        private const int ExpectedZeroItems = 0;

        private ReviewRepositoryMock _mockRepository;
        private IReviewsService _reviewsService;

        [Test]
        public void GetAllReviewsBySeller_ShouldReturnOnlySellerReviews()
        {
            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(_mockRepository);

            var seller1 = new User(Seller1Id, Seller1Name, Seller1Email);
            var seller2 = new User(Seller2Id, Seller2Name, Seller2Email);
            var buyer = new User(BuyerId, BuyerName, BuyerEmail);

            _mockRepository.CreateReview(new Review(Review1Id, Review1Description, new List<Image>(), Rating4_5, seller1.Id, buyer.Id));
            _mockRepository.CreateReview(new Review(Review2Id, Review2Description, new List<Image>(), Rating5_0, seller1.Id, buyer.Id));
            _mockRepository.CreateReview(new Review(Review3Id, Review3Description, new List<Image>(), Rating3_5, seller2.Id, buyer.Id));

            // Act
            var seller1Reviews = _reviewsService.GetReviewsBySeller(seller1);

            // Assert
            Assert.That(seller1Reviews.Count, Is.EqualTo(ExpectedTwoItems));
            Assert.That(seller1Reviews.All(r => r.SellerId == seller1.Id), Is.True);
        }

        [Test]
        public void GetReviewsByBuyer_ShouldReturnOnlyBuyerReviews()
        {
            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(_mockRepository);

            var seller = new User(Seller1Id, Seller1Name, Seller1Email);
            var buyer1 = new User(Buyer1Id, Seller2Name, Seller2Email); // Reusing Seller2 constants for buyer1
            var buyer2 = new User(Buyer2Id, BuyerName, BuyerEmail);

            _mockRepository.CreateReview(new Review(Review1Id, Review1Description, new List<Image>(), Rating4_5, seller.Id, buyer1.Id));
            _mockRepository.CreateReview(new Review(Review2Id, Review2Description, new List<Image>(), Rating5_0, seller.Id, buyer1.Id));
            _mockRepository.CreateReview(new Review(Review3Id, Review3Description, new List<Image>(), Rating3_5, seller.Id, buyer2.Id));

            // Act
            var buyerReviews = _reviewsService.GetReviewsByBuyer(buyer1);

            // Assert
            Assert.That(buyerReviews.Count, Is.EqualTo(ExpectedTwoItems));
            Assert.That(buyerReviews.All(r => r.BuyerId == buyer1.Id), Is.True);
        }

        [Test]
        public void AddReview_ShouldAddReviewToRepository()
        {
            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(_mockRepository);

            var seller = new User(Seller1Id, SellerLucaName, SellerLucaEmail);
            var buyer = new User(Buyer1Id, BuyerCristiName, BuyerCristiEmail);
            string description = LoveTestingDescription;
            List<Image> images = new List<Image>();
            float rating = Rating4_8;

            // Act
            _reviewsService.AddReview(description, images, rating, seller, buyer);

            // Assert
            Assert.That(_mockRepository.Reviews.Count, Is.EqualTo(ExpectedSingleItem));
            Assert.That(_mockRepository.Reviews[0].Description, Is.EqualTo(description));
            Assert.That(_mockRepository.Reviews[0].Rating, Is.EqualTo(rating));
            Assert.That(_mockRepository.Reviews[0].SellerId, Is.EqualTo(seller.Id));
            Assert.That(_mockRepository.Reviews[0].BuyerId, Is.EqualTo(buyer.Id));
        }

        [Test]
        public void EditReview_ShouldUpdateReviewInRepository()
        {
            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(_mockRepository);

            var seller = new User(Seller1Id, SellerLucaName, SellerLucaEmail);
            var buyer = new User(Buyer1Id, BuyerCristiName, BuyerCristiEmail);
            string originalDescription = LoveTestingDescription;
            string newDescription = LoveTestingNotDescription;
            float originalRating = Rating3_0;
            float newRating = Rating4_5;

            var review = new Review(Review1Id, originalDescription, new List<Image>(), originalRating, seller.Id, buyer.Id);
            _mockRepository.CreateReview(review);

            // Act
            _reviewsService.EditReview(originalDescription, new List<Image>(), originalRating, seller.Id, buyer.Id, newDescription, newRating);

            // Assert
            Assert.That(_mockRepository.Reviews.Count, Is.EqualTo(ExpectedSingleItem));
            var editedReview = _mockRepository.Reviews[0];
            Assert.That(editedReview.Description, Is.EqualTo(newDescription));
            Assert.That(editedReview.Rating, Is.EqualTo(newRating));
        }

        [Test]
        public void DeleteReview_ShouldRemoveReviewFromRepository()
        {
            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(_mockRepository);

            var seller = new User(Seller1Id, SellerLucaName, SellerLucaEmail);
            var buyer = new User(Buyer1Id, BuyerCristiName, BuyerCristiEmail);
            string originalDescription = LoveTestingDescription;
            float rating = Rating4_0;

            var review = new Review(Review1Id, originalDescription, new List<Image>(), rating, seller.Id, buyer.Id);
            _mockRepository.CreateReview(review);
            Assert.That(_mockRepository.Reviews.Count, Is.EqualTo(ExpectedSingleItem),
                "Precondition: Repository should have one review");

            // Act
            _reviewsService.DeleteReview(originalDescription, new List<Image>(), rating, seller.Id, buyer.Id);

            // Assert
            Assert.That(_mockRepository.Reviews.Count, Is.EqualTo(ExpectedZeroItems));
        }
    }
}

