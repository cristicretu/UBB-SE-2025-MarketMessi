using System;
using System.Collections.Generic;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Services.BuyProductsService;
using MarketMinds.Repositories.BuyProductsRepository;
using MarketMinds.Test.Services.BuyProductsServiceTest;

namespace MarketMinds.Tests.Services.BuyProductsServiceTest
{
    [TestFixture]
    public class BuyProductsServiceTest
    {
        // Constants to replace magic numbers
        private const int TestSellerId = 1;
        private const int TestBuyProductId = 1;
        private const int TestInvalidProductId = 2;
        private const int NonExistentProductId = 999;
        private const float TestProductPrice = 99.99f;
        private const float TestInvalidProductPrice = 100.0f;
        private const int ExpectedSingleCount = 1;
        private const int ExpectedZeroCount = 0;
        private const int AuctionDaysLength = 7;

        private BuyProductsService buyProductsService;
        private BuyProductsRepositoryMock buyProductsRepositoryMock;
        private User testSeller;
        private BuyProduct testBuyProduct;
        private AuctionProduct testInvalidProduct;

        [SetUp]
        public void Setup()
        {
            buyProductsRepositoryMock = new BuyProductsRepositoryMock();
            buyProductsService = new BuyProductsService(buyProductsRepositoryMock);

            testSeller = new User(TestSellerId, "Test Seller", "seller@test.com");

            var testCondition = new ProductCondition(1, "New", "Brand new item");
            var testCategory = new ProductCategory(1, "Electronics", "Electronic devices");
            var testTags = new List<ProductTag>();
            var testImages = new List<Image>();

            testBuyProduct = new BuyProduct(
                TestBuyProductId,
                "Test Buy Product",
                "Test Description",
                testSeller,
                testCondition,
                testCategory,
                testTags,
                testImages,
                TestProductPrice);

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
                DateTime.Now.AddDays(AuctionDaysLength),
                TestInvalidProductPrice);
        }

        [Test]
        public void TestCreateListing_ValidProduct_AddsProduct()
        {
            // Act
            buyProductsService.CreateListing(testBuyProduct);

            // Assert
            Assert.That(buyProductsRepositoryMock.GetCreateListingCount(), Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestCreateListing_InvalidProductType_ThrowsInvalidCastException()
        {
            // Act & Assert
            InvalidCastException thrownException = Assert.Throws<InvalidCastException>(() =>
                buyProductsService.CreateListing(testInvalidProduct));

            Assert.That(thrownException, Is.Not.Null);
        }

        [Test]
        public void TestCreateListing_InvalidProductType_DoesNotModifyRepository()
        {
            // Arrange & Act
            try
            {
                buyProductsService.CreateListing(testInvalidProduct);
            }
            catch
            {
                // Expected exception, ignore
            }

            // Assert
            Assert.That(buyProductsRepositoryMock.GetCreateListingCount(), Is.EqualTo(ExpectedZeroCount));
        }

        [Test]
        public void TestDeleteListing_ValidProduct_DeletesProduct()
        {
            // Act
            buyProductsService.DeleteListing(testBuyProduct);

            // Assert
            Assert.That(buyProductsRepositoryMock.GetDeleteListingCount(), Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestGetProducts_ReturnsNonNullResult()
        {
            // Arrange
            buyProductsRepositoryMock.AddProduct(testBuyProduct);

            // Act
            var products = buyProductsService.GetProducts();

            // Assert
            Assert.That(products, Is.Not.Null);
        }

        [Test]
        public void TestGetProducts_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            buyProductsRepositoryMock.AddProduct(testBuyProduct);

            // Act
            var products = buyProductsService.GetProducts();

            // Assert
            Assert.That(products.Count, Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestGetProducts_CallsRepositoryMethod()
        {
            // Arrange
            buyProductsRepositoryMock.AddProduct(testBuyProduct);

            // Act
            buyProductsService.GetProducts();

            // Assert
            Assert.That(buyProductsRepositoryMock.GetGetProductsCount(), Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestGetProductById_ValidId_ReturnsNonNullProduct()
        {
            // Arrange
            buyProductsRepositoryMock.AddProduct(testBuyProduct);

            // Act
            var product = buyProductsService.GetProductById(testBuyProduct.Id);

            // Assert
            Assert.That(product, Is.Not.Null);
        }

        [Test]
        public void TestGetProductById_ValidId_ReturnsProductWithCorrectId()
        {
            // Arrange
            buyProductsRepositoryMock.AddProduct(testBuyProduct);

            // Act
            var product = buyProductsService.GetProductById(testBuyProduct.Id);

            // Assert
            Assert.That(product.Id, Is.EqualTo(testBuyProduct.Id));
        }

        [Test]
        public void TestGetProductById_ValidId_CallsRepositoryMethod()
        {
            // Arrange
            buyProductsRepositoryMock.AddProduct(testBuyProduct);

            // Act
            buyProductsService.GetProductById(testBuyProduct.Id);

            // Assert
            Assert.That(buyProductsRepositoryMock.GetGetProductByIdCount(), Is.EqualTo(ExpectedSingleCount));
        }

        [Test]
        public void TestGetProductById_InvalidId_ReturnsNull()
        {
            // Act
            var product = buyProductsService.GetProductById(NonExistentProductId);

            // Assert
            Assert.That(product, Is.Null);
        }

        [Test]
        public void TestGetProductById_InvalidId_CallsRepositoryMethod()
        {
            // Act
            buyProductsService.GetProductById(NonExistentProductId);

            // Assert
            Assert.That(buyProductsRepositoryMock.GetGetProductByIdCount(), Is.EqualTo(ExpectedSingleCount));
        }
    }
}
