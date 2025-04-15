using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DomainLayer.Domain;
using ViewModelLayer.ViewModel;
using BusinessLogicLayer.ViewModel;
using MarketMinds.Helpers;
using MarketMinds.Services;
using MarketMinds.Services.ProductTagService;
using MarketMinds.Services.BuyProductsService;
using Moq;
using MarketMinds.Services.ProductPaginationService;

namespace MarketMinds.Test.Helpers.BuyProductListViewModelHelperTest
{
    [TestFixture]
    public class BuyProductListViewModelHelperTest
    {
        private BuyProductListViewModelHelper buyProductListViewModelHelper;
        private BuyProductsViewModel buyProductsViewModel;
        private SortAndFilterViewModel sortAndFilterViewModel;
        private List<BuyProduct> testProducts;
        private Mock<ProductPaginationService> mockPaginationService;

        [SetUp]
        public void Setup()
        {
            buyProductListViewModelHelper = new BuyProductListViewModelHelper();
            
            // Create 30 test products (more than default page size)
            testProducts = BuyProductListViewModelHelperMocks.CreateTestBuyProducts(30);
            
            // Create SortAndFilterViewModel with a mocked product service
            sortAndFilterViewModel = BuyProductListViewModelHelperMocks.CreateSortAndFilterViewModel(
                testProducts.Cast<Product>().ToList());
                
            // Create a real BuyProductsViewModel
            var mockBuyProductsService = new Mock<BuyProductsService>(null);
            buyProductsViewModel = new BuyProductsViewModel(mockBuyProductsService.Object);
        }

        [Test]
        public void TestGetBuyProductsPage_ReturnsCorrectData()
        {
            // Arrange
            int currentPage = 1;
            
            // Act
            var result = buyProductListViewModelHelper.GetBuyProductsPage(
                buyProductsViewModel,
                sortAndFilterViewModel,
                currentPage);
            
            // Assert
            Assert.That(result.currentPageProducts, Is.Not.Null);
            Assert.That(result.fullList, Is.Not.Null);
            Assert.That(result.fullList.Count, Is.EqualTo(30));
        }

        [Test]
        public void TestGetBuyProductsPage_EmptyList_ReturnsEmptyData()
        {
            // Arrange
            int currentPage = 1;
            sortAndFilterViewModel = BuyProductListViewModelHelperMocks.CreateSortAndFilterViewModel(
                new List<Product>());
            
            // Act
            var result = buyProductListViewModelHelper.GetBuyProductsPage(
                buyProductsViewModel,
                sortAndFilterViewModel,
                currentPage);
            
            // Assert
            Assert.That(result.currentPageProducts, Is.Not.Null);
            Assert.That(result.fullList, Is.Not.Null);
            Assert.That(result.fullList.Count, Is.EqualTo(0));
            Assert.That(result.totalPages, Is.EqualTo(0));
        }

        [Test]
        public void TestGetBuyProductsPage_SecondPage_ReturnsCorrectData()
        {
            // Arrange
            int currentPage = 2;
            
            // Act
            var result = buyProductListViewModelHelper.GetBuyProductsPage(
                buyProductsViewModel,
                sortAndFilterViewModel,
                currentPage);
            
            // Assert
            Assert.That(result.currentPageProducts, Is.Not.Null);
            Assert.That(result.fullList, Is.Not.Null);
            Assert.That(result.fullList.Count, Is.EqualTo(30));
            Assert.That(result.totalPages, Is.GreaterThan(1));
        }
    }
} 