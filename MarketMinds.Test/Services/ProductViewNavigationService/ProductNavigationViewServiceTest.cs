using System;
using System.Collections.Generic;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Services;
using MarketMinds.Views.Pages;
using Microsoft.UI.Xaml;
using Moq;
using BusinessLogicLayer.ViewModel;

namespace MarketMinds.Test.Services.ProductViewNavigationService
{
    [TestFixture]
    public class ProductViewNavigationServiceTest
    {
        // Constants to replace magic values
        private const int SellerId = 1;
        private const string SellerUsername = "TestSeller";
        private const string SellerEmail = "test@example.com";
        private const string ArgumentNameProduct = "product";
        private const string ArgumentNameSeller = "seller";

        private MarketMinds.Services.ProductViewNavigationService _navigationService;
        private User _seller;
        private BuyProduct _testProduct;

        [SetUp]
        public void Setup()
        {
            _navigationService = new MarketMinds.Services.ProductViewNavigationService();
            _seller = new User(SellerId, SellerUsername, SellerEmail);

            // Initialize a test product if needed for non-null tests
            _testProduct = CreateTestProduct();
        }

        #region CreateProductDetailView Tests

        [Test]
        public void CreateProductDetailView_WithNullProduct_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                _navigationService.CreateProductDetailView(null));

            // Additional assertion can be in a separate test
            Assert.That(exception, Is.Not.Null);
        }

        [Test]
        public void CreateProductDetailView_WithNullProduct_HasCorrectParamName()
        {
            // Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
                _navigationService.CreateProductDetailView(null));

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo(ArgumentNameProduct));
        }

        #endregion

        #region CreateSellerReviewsView Tests

        [Test]
        public void CreateSellerReviewsView_WithNullSeller_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                _navigationService.CreateSellerReviewsView(null));

            // Additional assertion can be in a separate test
            Assert.That(exception, Is.Not.Null);
        }

        [Test]
        public void CreateSellerReviewsView_WithNullSeller_HasCorrectParamName()
        {
            // Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
                _navigationService.CreateSellerReviewsView(null));

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo(ArgumentNameSeller));
        }

        #endregion

        #region Helper Methods

        private BuyProduct CreateTestProduct()
        {
            // Create a minimal valid product for testing non-null scenarios
            // This is a helper method that can be expanded if needed for additional tests
            var category = new ProductCategory(1, "Test Category", "Test Category Description");
            var condition = new ProductCondition(1, "New", "Brand new item");

            return new BuyProduct(
                1,
                "Test Product",
                "Test Description",
                _seller,
                condition,
                category,
                new List<ProductTag>(),
                new List<Image>(),
                99.99f
            );
        }

        #endregion
    }
}