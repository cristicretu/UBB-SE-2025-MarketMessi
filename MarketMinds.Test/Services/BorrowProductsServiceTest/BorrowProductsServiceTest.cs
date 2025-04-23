using System;
using System.Collections.Generic;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Services.BorrowProductsService;
using MarketMinds.Repositories.BorrowProductsRepository;
using MarketMinds.Test.Services.BorrowProductsServiceTest;

namespace MarketMinds.Tests.Services.BorrowProductsServiceTest
{
    [TestFixture]
    public class BorrowProductsServiceTest
    {
        // Constants to replace magic numbers
        private const int TestSellerId = 1;
        private const int TestBorrowProductId = 1;
        private const int TestInvalidProductId = 2;
        private const int NonExistentProductId = 999;
        private const float DailyRateAmount = 20.0f;
        private const float StartingPriceAmount = 100.0f;
        private const int ExpectedSingleCount = 1;
        private const int ExpectedZeroCount = 0;
        private const int TimeSpanDaysLong = 7;
        private const int TimeSpanDaysMedium = 5;

        private BorrowProductsService borrowProductsService;
        private BorrowProductsRepositoryMock borrowProductsRepositoryMock;
        private User testSeller;
        private BorrowProduct testBorrowProduct;
        private AuctionProduct testInvalidProduct;

        [SetUp]
        public void Setup()
        {
            borrowProductsRepositoryMock = new BorrowProductsRepositoryMock();
            borrowProductsService = new BorrowProductsService(borrowProductsRepositoryMock);

            testSeller = new User(TestSellerId, "Test Seller", "seller@test.com");

            var testCondition = new ProductCondition(1, "New", "Brand new item");
            var testCategory = new ProductCategory(1, "Electronics", "Electronic devices");
            var testTags = new List<ProductTag>();
            var testImages = new List<Image>();

            DateTime timeLimit = DateTime.Now.AddDays(TimeSpanDaysLong);
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now.AddDays(TimeSpanDaysMedium);
            bool isBorrowed = false;

            testBorrowProduct = new BorrowProduct(
                TestBorrowProductId,
                "Test Borrow Product",
                "Test Description",
                testSeller,
                testCondition,
                testCategory,
                testTags,
                testImages,
                timeLimit,
                startDate,
                endDate,
                DailyRateAmount,
                isBorrowed);

            // Create an invalid product type for testing type validation
            testInvalidProduct = new AuctionProduct(
                TestInvalidProductId,
                "Test Auction Product",
                "Test Description",
                testSeller,
                testCondition,
                testCategory,
                testTags,
                testImages,
                DateTime.Now,
                DateTime.Now.AddDays(TimeSpanDaysLong),
                StartingPriceAmount);
        }

        [Test]
        public void TestCreateListing_ValidProduct_AddsProduct()
        {
            // Act
            borrowProductsService.CreateListing(testBorrowProduct);

            // Assert
            Assert.That(borrowProductsRepositoryMock.GetCreateListingCount(), Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestCreateListing_InvalidProductType_ThrowsInvalidCastException()
        {
            // Arrange & Act & Assert
            InvalidCastException castException = Assert.Throws<InvalidCastException>(() =>
                borrowProductsService.CreateListing(testInvalidProduct));

            Assert.That(castException, Is.Not.Null);
        }

        [Test]
        public void TestCreateListing_InvalidProductType_DoesNotAddToRepository()
        {
            // Arrange & Act
            try
            {
                borrowProductsService.CreateListing(testInvalidProduct);
            }
            catch
            {
                // Expected exception, ignore
            }

            // Assert
            Assert.That(borrowProductsRepositoryMock.GetCreateListingCount(), Is.EqualTo(ExpectedZeroCount));
        }

        [Test]
        public void TestDeleteListing_ValidProduct_DeletesProduct()
        {
            // Act
            borrowProductsService.DeleteListing(testBorrowProduct);

            // Assert
            Assert.That(borrowProductsRepositoryMock.GetDeleteListingCount(), Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestGetProducts_ReturnsNonNullProductsList()
        {
            // Arrange
            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

            // Act
            var products = borrowProductsService.GetProducts();

            // Assert
            Assert.That(products, Is.Not.Null);
        }

        [Test]
        public void TestGetProducts_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

            // Act
            var products = borrowProductsService.GetProducts();

            // Assert
            Assert.That(products.Count, Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestGetProducts_CallsRepositoryCorrectly()
        {
            // Arrange
            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

            // Act
            borrowProductsService.GetProducts();

            // Assert
            Assert.That(borrowProductsRepositoryMock.GetGetProductsCount(), Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestGetProductById_ValidId_ReturnsNonNullProduct()
        {
            // Arrange
            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

            // Act
            var product = borrowProductsService.GetProductById(testBorrowProduct.Id);

            // Assert
            Assert.That(product, Is.Not.Null);
        }

        [Test]
        public void TestGetProductById_ValidId_ReturnsProductWithCorrectId()
        {
            // Arrange
            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

            // Act
            var product = borrowProductsService.GetProductById(testBorrowProduct.Id);

            // Assert
            Assert.That(product.Id, Is.EqualTo(testBorrowProduct.Id));
        }

        [Test]
        public void TestGetProductById_ValidId_CallsRepositoryCorrectly()
        {
            // Arrange
            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

            // Act
            borrowProductsService.GetProductById(testBorrowProduct.Id);

            // Assert
            Assert.That(borrowProductsRepositoryMock.GetGetProductByIdCount(), Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestGetProductById_InvalidId_ReturnsNull()
        {
            // Act
            var product = borrowProductsService.GetProductById(NonExistentProductId);

            // Assert
            Assert.That(product, Is.Null);
        }

        [Test]
        public void TestGetProductById_InvalidId_CallsRepositoryCorrectly()
        {
            // Act
            borrowProductsService.GetProductById(NonExistentProductId);

            // Assert
            Assert.That(borrowProductsRepositoryMock.GetGetProductByIdCount(), Is.EqualTo(ExpectedSingleCount));
        }
    }
}