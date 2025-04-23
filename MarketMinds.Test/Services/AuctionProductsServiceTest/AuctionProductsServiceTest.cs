using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Services.AuctionProductsService;
using MarketMinds.Repositories;
using MarketMinds.Repositories.AuctionProductsRepository;
using MarketMinds.Test.Services.AuctionProductsServiceTest;
using Moq; 


namespace MarketMinds.Tests.Services.AuctionProductsServiceTest
{
    [TestFixture]
    public class AuctionProductsServiceTest
    {
        private AuctionProductsService auctionProductsService;
        private AuctionProductsRepositoryMock auctionProductsRepositoryMock;

        // Constants to replace magic numbers
        private const int SellerId = 1;
        private const int BidderId = 2;
        private const int FirstBidderId = 3;
        private const float DefaultBidderBalance = 1000f;
        private const float InitialBidAmount = 200f;
        private const float LowBidAmount = 150f;
        private const float InsufficientBalanceAmount = 50f;
        private const float StartingPrice = 100f;
        private const float SecondBidAmount = 200f;
        private const float FirstBidAmount = 150f;
        private const float FirstBidderBalance = 500f;
        private const int ExpectedSingleUpdate = 1;
        private const int ExpectedNoUpdate = 0;
        private const int ExpectedDoubleUpdate = 2;
        private const int ExpectedSingleItem = 1;
        private const int BidNearEndTimeMinutes = 3;

        User testSeller;
        User testBidder;
        ProductCondition testProductCondition;
        ProductCategory testProductCategory;
        List<ProductTag> testProductTags;
        AuctionProduct testAuction;

        [SetUp]
        public void Setup()
        {
            auctionProductsRepositoryMock = new AuctionProductsRepositoryMock();
            auctionProductsService = new AuctionProductsService(auctionProductsRepositoryMock);

            testSeller = new User(SellerId, "test seller", "seller@test.com");
            testBidder = new User(BidderId, "test bidder", "bidder@test.com");
            testBidder.Balance = DefaultBidderBalance;
            testProductCondition = new ProductCondition(1, "Test", "Test");
            testProductCategory = new ProductCategory(1, "test", "test");
            testProductTags = new List<ProductTag>();

            testAuction = new AuctionProduct(
                1,
                "Test Auction",
                "Test Description",
                testSeller,
                testProductCondition,
                testProductCategory,
                testProductTags,
                new List<Image>(),
                DateTime.Now.AddDays(-1),
                DateTime.Now.AddDays(1),
                StartingPrice);
        }

        [Test]
        public void TestPlaceBid_ValidBid_UpdatesAuctionPrice()
        {
            // Arrange
            float bidAmount = InitialBidAmount;

            // Act
            auctionProductsService.PlaceBid(testAuction, testBidder, bidAmount);

            // Assert
            Assert.That(testAuction.CurrentPrice, Is.EqualTo(bidAmount));
            Assert.That(testAuction.BidHistory[0].Price, Is.EqualTo(bidAmount));
        }

        [Test]
        public void TestPlaceBid_ValidBid_UpdatesBidderBalance()
        {
            // Arrange
            float bidAmount = InitialBidAmount;
            float expectedBalance = DefaultBidderBalance - bidAmount;

            // Act
            auctionProductsService.PlaceBid(testAuction, testBidder, bidAmount);

            // Assert
            Assert.That(testBidder.Balance, Is.EqualTo(expectedBalance));
        }

        [Test]
        public void TestPlaceBid_ValidBid_AddsToBidHistory()
        {
            // Arrange
            float bidAmount = InitialBidAmount;

            // Act
            auctionProductsService.PlaceBid(testAuction, testBidder, bidAmount);

            // Assert
            Assert.That(testAuction.BidHistory.Count, Is.EqualTo(ExpectedSingleItem));
            Assert.That(testAuction.BidHistory[0].Bidder, Is.EqualTo(testBidder));
        }

        [Test]
        public void TestPlaceBid_ValidBid_UpdatesRepository()
        {
            // Arrange
            float bidAmount = InitialBidAmount;

            // Act
            auctionProductsService.PlaceBid(testAuction, testBidder, bidAmount);

            // Assert
            Assert.That(auctionProductsRepositoryMock.GetUpdateCount(), Is.EqualTo(ExpectedSingleUpdate));
        }

        [Test]
        public void TestPlaceBid_BidTooLow_ThrowsException()
        {
            // Arrange
            PlaceInitialBid();
            ResetRepositoryAndService();
            // Act & Assert
            Exception exceptionPlacingLowBid = AssertThrowsWhenPlacingLowBid();
            Assert.That(exceptionPlacingLowBid.Message, Does.Contain("Bid must be at least"));
        }

        [Test]
        public void TestPlaceBid_BidTooLow_DoesNotUpdateRepository()
        {
            // Arrange
            PlaceInitialBid();
            ResetRepositoryAndService();
            // Act
           auctionProductsService.PlaceBid(testAuction, testBidder, LowBidAmount);
            // Assert
            Assert.That(auctionProductsRepositoryMock.GetUpdateCount(), Is.EqualTo(ExpectedNoUpdate));
        }

        [Test]
        public void TestPlaceBid_InsufficientBalance_ThrowsException()
        {
            // Arrange
            testBidder.Balance = InsufficientBalanceAmount;

            // Act & Assert
            Exception ex = AssertThrowsWhenPlacingBid(InitialBidAmount);
            Assert.That(ex.Message, Is.EqualTo("Insufficient balance"));
        }

        [Test]
        public void TestPlaceBid_InsufficientBalance_DoesNotUpdateRepository()
        {
            // Arrange
            testBidder.Balance = InsufficientBalanceAmount;

            // Act
            try
            {
                auctionProductsService.PlaceBid(testAuction, testBidder, InitialBidAmount);
            }
            catch
            {
                // Expected exception, ignore
            }

            // Assert
            Assert.That(auctionProductsRepositoryMock.GetUpdateCount(), Is.EqualTo(ExpectedNoUpdate));
        }

        [Test]
        public void TestPlaceBid_AuctionEnded_ThrowsException()
        {
            // Arrange
            SetAuctionAsEnded();

            // Act & Assert
            Exception ex = AssertThrowsWhenPlacingBid(InitialBidAmount);
            Assert.That(ex.Message, Is.EqualTo("Auction already ended"));
        }

        [Test]
        public void TestPlaceBid_AuctionEnded_DoesNotUpdateRepository()
        {
            // Arrange
            SetAuctionAsEnded();

            // Act
            try
            {
                auctionProductsService.PlaceBid(testAuction, testBidder, InitialBidAmount);
            }
            catch
            {
                // Expected exception, ignore
            }

            // Assert
            Assert.That(auctionProductsRepositoryMock.GetUpdateCount(), Is.EqualTo(ExpectedNoUpdate));
        }

        [Test]
        public void TestPlaceBid_MultipleBids_RefundsPreviousBidder()
        {
            // Arrange
            User firstBidder = CreateFirstBidder();
            PlaceFirstBid(firstBidder);

            // Act
            PlaceSecondBid();

            // Assert
            Assert.That(firstBidder.Balance, Is.EqualTo(FirstBidderBalance)); // Should be fully refunded
        }

        [Test]
        public void TestPlaceBid_MultipleBids_UpdatesAuctionAndBidHistory()
        {
            // Arrange
            User firstBidder = CreateFirstBidder();
            PlaceFirstBid(firstBidder);

            // Act
            PlaceSecondBid();

            // Assert
            Assert.That(testBidder.Balance, Is.EqualTo(DefaultBidderBalance - SecondBidAmount));
            Assert.That(testAuction.CurrentPrice, Is.EqualTo(SecondBidAmount));
            Assert.That(testAuction.BidHistory.Count, Is.EqualTo(ExpectedDoubleUpdate));
        }

        [Test]
        public void TestPlaceBid_MultipleBids_UpdatesRepositoryTwice()
        {
            // Arrange
            User firstBidder = CreateFirstBidder();
            PlaceFirstBid(firstBidder);

            // Act
            PlaceSecondBid();

            // Assert
            Assert.That(auctionProductsRepositoryMock.GetUpdateCount(), Is.EqualTo(ExpectedDoubleUpdate));
        }

        [Test]
        public void TestPlaceBid_BidNearEndTime_ExtendsAuctionTime()
        {
            // Arrange
            SetAuctionCloseToEnding();
            DateTime originalEndTime = testAuction.EndAuctionDate;

            // Act
            auctionProductsService.PlaceBid(testAuction, testBidder, InitialBidAmount);

            // Assert
            Assert.That(testAuction.EndAuctionDate, Is.GreaterThan(originalEndTime));
        }

        [Test]
        public void TestPlaceBid_BidNearEndTime_UpdatesRepository()
        {
            // Arrange
            SetAuctionCloseToEnding();

            // Act
            auctionProductsService.PlaceBid(testAuction, testBidder, InitialBidAmount);

            // Assert
            Assert.That(auctionProductsRepositoryMock.GetUpdateCount(), Is.EqualTo(ExpectedSingleUpdate));
        }

        [Test]
        public void TestConcludeAuction_ValidAuction_DeletesAuction()
        {
            // Act
            auctionProductsService.ConcludeAuction(testAuction);

            // Assert
            Assert.That(auctionProductsRepositoryMock.GetDeleteCount(), Is.EqualTo(ExpectedSingleItem));
        }

        [Test]
        public void TestCreateListing_AddsProductToRepository()
        {
            // Act
            auctionProductsService.CreateListing(testAuction);

            // Assert
            Assert.That(auctionProductsRepositoryMock.GetProducts().Count, Is.EqualTo(ExpectedSingleItem));
        }

        [Test]
        public void TestCreateListing_AddsCorrectProduct()
        {
            // Act
            auctionProductsService.CreateListing(testAuction);

            // Assert
            Assert.That(auctionProductsRepositoryMock.GetProducts()[0], Is.EqualTo(testAuction));
        }

        #region Helper Methods
        private void PlaceInitialBid()
        {
            auctionProductsService.PlaceBid(testAuction, testBidder, InitialBidAmount);
        }

        private void ResetRepositoryAndService()
        {
            auctionProductsRepositoryMock = new AuctionProductsRepositoryMock();
            auctionProductsService = new AuctionProductsService(auctionProductsRepositoryMock);
        }

        private Exception AssertThrowsWhenPlacingLowBid()
        {
            Exception ex = null;
            try
            {
                auctionProductsService.PlaceBid(testAuction, testBidder, LowBidAmount);
            }
            catch (Exception e)
            {
                ex = e;
                Console.WriteLine($"Exception caught: {e.Message}");
            }

            Assert.That(ex, Is.Not.Null, "Expected an exception for bid too low but none was thrown");
            return ex;
        }

        private Exception AssertThrowsWhenPlacingBid(float bidAmount)
        {
            Exception ex = null;
            try
            {
                auctionProductsService.PlaceBid(testAuction, testBidder, bidAmount);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.That(ex, Is.Not.Null);
            return ex;
        }

        private void SetAuctionAsEnded()
        {
            testAuction.EndAuctionDate = DateTime.Now.AddDays(-1);
        }

        private User CreateFirstBidder()
        {
            User firstBidder = new User(FirstBidderId, "first bidder", "first@test.com");
            firstBidder.Balance = FirstBidderBalance;
            return firstBidder;
        }

        private void PlaceFirstBid(User firstBidder)
        {
            auctionProductsService.PlaceBid(testAuction, firstBidder, FirstBidAmount);
        }

        private void PlaceSecondBid()
        {
            auctionProductsService.PlaceBid(testAuction, testBidder, SecondBidAmount);
        }

        private void SetAuctionCloseToEnding()
        {
            testAuction.EndAuctionDate = DateTime.Now.AddMinutes(BidNearEndTimeMinutes);
        }
        #endregion
    }
}