using DomainLayer.Domain;
using MarketMinds.Services;
using MarketMinds.Services.AuctionProductsService;
using MarketMinds.Services.BorrowProductsService;
using MarketMinds.Services.BuyProductsService;
using Moq;
using NUnit.Framework;
using System;
using MarketMinds.Services.ListingCreationService;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class ListingCreationServiceTest
    {
        // Constants for listing types
        private const string BuyListingType = "buy";
        private const string BorrowListingType = "borrow";
        private const string AuctionListingType = "auction";
        private const string InvalidListingType = "invalid_type";
        private const string UppercaseBuyListingType = "BUY";
        private const string InvalidListingTypeErrorMessage = "Invalid listing type";

        private Mock<IBuyProductsService> _mockBuyService;
        private Mock<IBorrowProductsService> _mockBorrowService;
        private Mock<IAuctionProductsService> _mockAuctionService;
        private ListingCreationService _service;
        private Product _testProduct;

        [SetUp]
        public void Setup()
        {
            _mockBuyService = new Mock<IBuyProductsService>();
            _mockBorrowService = new Mock<IBorrowProductsService>();
            _mockAuctionService = new Mock<IAuctionProductsService>();

            _service = new ListingCreationService(
                _mockBuyService.Object,
                _mockBorrowService.Object,
                _mockAuctionService.Object);
        }

        [Test]
        public void CreateMarketListing_BuyType_CallsBuyProductsService()
        {
            // Act
            _service.CreateMarketListing(_testProduct, BuyListingType);

            // Assert
            _mockBuyService.Verify(s => s.CreateListing(_testProduct), Times.Once);
        }

        [Test]
        public void CreateMarketListing_BuyType_DoesNotCallOtherServices()
        {
            // Act
            _service.CreateMarketListing(_testProduct, BuyListingType);

            // Assert
            _mockBorrowService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
            _mockAuctionService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        }

        [Test]
        public void CreateMarketListing_BorrowType_CallsBorrowProductsService()
        {
            // Act
            _service.CreateMarketListing(_testProduct, BorrowListingType);

            // Assert
            _mockBorrowService.Verify(s => s.CreateListing(_testProduct), Times.Once);
        }

        [Test]
        public void CreateMarketListing_BorrowType_DoesNotCallOtherServices()
        {
            // Act
            _service.CreateMarketListing(_testProduct, BorrowListingType);

            // Assert
            _mockBuyService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
            _mockAuctionService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        }

        [Test]
        public void CreateMarketListing_AuctionType_CallsAuctionProductsService()
        {
            // Act
            _service.CreateMarketListing(_testProduct, AuctionListingType);

            // Assert
            _mockAuctionService.Verify(s => s.CreateListing(_testProduct), Times.Once);
        }

        [Test]
        public void CreateMarketListing_AuctionType_DoesNotCallOtherServices()
        {
            // Act
            _service.CreateMarketListing(_testProduct, AuctionListingType);

            // Assert
            _mockBuyService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
            _mockBorrowService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        }

        [Test]
        public void CreateMarketListing_InvalidType_ThrowsArgumentException()
        {
            // Act & Assert
            ArgumentException thrownException = Assert.Throws<ArgumentException>(() =>
                _service.CreateMarketListing(_testProduct, InvalidListingType));

            Assert.That(thrownException, Is.Not.Null);
        }

        [Test]
        public void CreateMarketListing_InvalidType_ContainsExpectedErrorMessage()
        {
            // Act & Assert
            ArgumentException thrownException = Assert.Throws<ArgumentException>(() =>
                _service.CreateMarketListing(_testProduct, InvalidListingType));

            Assert.That(thrownException.Message, Does.Contain(InvalidListingTypeErrorMessage));
        }

        [Test]
        public void CreateMarketListing_CaseInsensitive_CallsCorrectService()
        {
            // Act
            _service.CreateMarketListing(_testProduct, UppercaseBuyListingType);

            // Assert
            _mockBuyService.Verify(s => s.CreateListing(_testProduct), Times.Once);
        }
    }
}