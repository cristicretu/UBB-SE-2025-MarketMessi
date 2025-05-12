//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using MarketMinds.Shared.Models;
//using MarketMinds.Shared.Services.BuyProductsService;
//using MarketMinds.Repositories.BuyProductsRepository;
//using MarketMinds.Test.Services.BuyProductsServiceTest;

//namespace MarketMinds.Tests.Services.BuyProductsServiceTest
//{
//    [TestFixture]
//    public class BuyProductsServiceTest
//    {
//        // Constants to replace magic numbers
//        private const int TEST_SELLER_ID = 1;
//        private const int TEST_BUY_PRODUCT_ID = 1;
//        private const int TEST_INVALID_PRODUCT_ID = 2;
//        private const int NON_EXISTENT_PRODUCT_ID = 999;
//        private const float TEST_PRODUCT_PRICE = 99.99f;
//        private const float TEST_INVALID_PRODUCT_PRICE = 100.0f;
//        private const int EXPECTED_SINGLE_COUNT = 1;
//        private const int EXPECTED_ZERO_COUNT = 0;
//        private const int AUCTION_DAYS_LENGTH = 7;

//        private BuyProductsService buyProductsService;
//        private BuyProductsRepositoryMock buyProductsRepositoryMock;
//        private User testSeller;
//        private BuyProduct testBuyProduct;
//        private AuctionProduct testInvalidProduct;

//        [SetUp]
//        public void Setup()
//        {
//            buyProductsRepositoryMock = new BuyProductsRepositoryMock();
//            buyProductsService = new BuyProductsService(buyProductsRepositoryMock);

//            testSeller = new User(TEST_SELLER_ID, "Test Seller", "seller@test.com");

//            var testCondition = new Condition(1, "New", "Brand new item");
//            var testCategory = new Category(1, "Electronics", "Electronic devices");
//            var testTags = new List<ProductTag>();
//            var testImages = new List<Image>();

//            testBuyProduct = new BuyProduct(
//                TEST_BUY_PRODUCT_ID,
//                "Test Buy Product",
//                "Test Description",
//                testSeller,
//                testCondition,
//                testCategory,
//                testTags,
//                testImages,
//                TEST_PRODUCT_PRICE);

//            // Create an invalid product type for testing type validation
//            testInvalidProduct = new AuctionProduct(
//                TEST_INVALID_PRODUCT_ID,
//                "Test Auction Product",
//                "Test Description",
//                testSeller,
//                testCondition,
//                testCategory,
//                testTags,
//                testImages,
//                DateTime.Now,
//                DateTime.Now.AddDays(AUCTION_DAYS_LENGTH),
//                TEST_INVALID_PRODUCT_PRICE);
//        }

//        [Test]
//        public void TestCreateListing_ValidProduct_AddsProduct()
//        {
//            // Act
//            buyProductsService.CreateListing(testBuyProduct);

//            // Assert
//            Assert.That(buyProductsRepositoryMock.GetCreateListingCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestCreateListing_InvalidProductType_ThrowsInvalidCastException()
//        {
//            // Act & Assert
//            InvalidCastException thrownException = Assert.Throws<InvalidCastException>(() =>
//                buyProductsService.CreateListing(testInvalidProduct));

//            Assert.That(thrownException, Is.Not.Null);
//        }

//        [Test]
//        public void TestCreateListing_InvalidProductType_DoesNotModifyRepository()
//        {
//            // Arrange & Act
//            try
//            {
//                buyProductsService.CreateListing(testInvalidProduct);
//            }
//            catch
//            {
//                // Expected exception, ignore
//            }

//            // Assert
//            Assert.That(buyProductsRepositoryMock.GetCreateListingCount(), Is.EqualTo(EXPECTED_ZERO_COUNT));
//        }

//        [Test]
//        public void TestDeleteListing_ValidProduct_DeletesProduct()
//        {
//            // Act
//            buyProductsService.DeleteListing(testBuyProduct);

//            // Assert
//            Assert.That(buyProductsRepositoryMock.GetDeleteListingCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestGetProducts_ReturnsNonNullResult()
//        {
//            // Arrange
//            buyProductsRepositoryMock.AddProduct(testBuyProduct);

//            // Act
//            var products = buyProductsService.GetProducts();

//            // Assert
//            Assert.That(products, Is.Not.Null);
//        }

//        [Test]
//        public void TestGetProducts_ReturnsCorrectNumberOfProducts()
//        {
//            // Arrange
//            buyProductsRepositoryMock.AddProduct(testBuyProduct);

//            // Act
//            var products = buyProductsService.GetProducts();

//            // Assert
//            Assert.That(products.Count, Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestGetProducts_CallsRepositoryMethod()
//        {
//            // Arrange
//            buyProductsRepositoryMock.AddProduct(testBuyProduct);

//            // Act
//            buyProductsService.GetProducts();

//            // Assert
//            Assert.That(buyProductsRepositoryMock.GetGetProductsCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestGetProductById_ValidId_ReturnsNonNullProduct()
//        {
//            // Arrange
//            buyProductsRepositoryMock.AddProduct(testBuyProduct);

//            // Act
//            var product = buyProductsService.GetProductById(testBuyProduct.Id);

//            // Assert
//            Assert.That(product, Is.Not.Null);
//        }

//        [Test]
//        public void TestGetProductById_ValidId_ReturnsProductWithCorrectId()
//        {
//            // Arrange
//            buyProductsRepositoryMock.AddProduct(testBuyProduct);

//            // Act
//            var product = buyProductsService.GetProductById(testBuyProduct.Id);

//            // Assert
//            Assert.That(product.Id, Is.EqualTo(testBuyProduct.Id));
//        }

//        [Test]
//        public void TestGetProductById_ValidId_CallsRepositoryMethod()
//        {
//            // Arrange
//            buyProductsRepositoryMock.AddProduct(testBuyProduct);

//            // Act
//            buyProductsService.GetProductById(testBuyProduct.Id);

//            // Assert
//            Assert.That(buyProductsRepositoryMock.GetGetProductByIdCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestGetProductById_InvalidId_ReturnsNull()
//        {
//            // Act
//            var product = buyProductsService.GetProductById(NON_EXISTENT_PRODUCT_ID);

//            // Assert
//            Assert.That(product, Is.Null);
//        }

//        [Test]
//        public void TestGetProductById_InvalidId_CallsRepositoryMethod()
//        {
//            // Act
//            buyProductsService.GetProductById(NON_EXISTENT_PRODUCT_ID);

//            // Assert
//            Assert.That(buyProductsRepositoryMock.GetGetProductByIdCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }
//    }
//}