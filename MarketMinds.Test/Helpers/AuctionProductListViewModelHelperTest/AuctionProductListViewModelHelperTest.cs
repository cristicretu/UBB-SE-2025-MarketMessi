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
using MarketMinds.Services.AuctionProductsService;
using Moq;

namespace MarketMinds.Test.Helpers.AuctionProductListViewModelHelperTest
{
    [TestFixture]
    public class AuctionProductListViewModelHelperTest
    {
        private AuctionProductListViewModelHelper auctionProductListViewModelHelper;
        private AuctionProductsViewModel auctionProductsViewModel;
        private SortAndFilterViewModel sortAndFilterViewModel;
        private List<AuctionProduct> testProducts;
        private const int BASE_PAGE = 1;
        private const int ITEMS_PER_PAGE = 20;

        [SetUp]
        public void Setup()
        {
            auctionProductListViewModelHelper = new AuctionProductListViewModelHelper();
            
            // Create 30 test products (more than default page size)
            testProducts = AuctionProductListViewModelHelperMocks.CreateTestAuctionProducts(30);
            
            // Create SortAndFilterViewModel with a mocked product service
            sortAndFilterViewModel = AuctionProductListViewModelHelperMocks.CreateSortAndFilterViewModel(
                testProducts.Cast<Product>().ToList());
                
            // Create a real AuctionProductsViewModel
            var mockAuctionProductsService = new Mock<AuctionProductsService>(null);
            auctionProductsViewModel = new AuctionProductsViewModel(mockAuctionProductsService.Object);
        }

        [Test]
        public void TestGetAuctionProductsPage_FirstPage_ReturnsCorrectItems()
        {
            // Arrange
            int currentPage = 1;
            
            // Act
            var result = auctionProductListViewModelHelper.GetAuctionProductsPage(
                auctionProductsViewModel,
                sortAndFilterViewModel,
                currentPage,
                ITEMS_PER_PAGE);
            
            // Assert
            Assert.That(result.pageItems, Is.Not.Null);
            Assert.That(result.pageItems.Count, Is.EqualTo(ITEMS_PER_PAGE)); // First 20 items
            Assert.That(result.totalPages, Is.EqualTo(2)); // 30 items / 20 per page = 2 pages
            Assert.That(result.fullList.Count, Is.EqualTo(30));
            Assert.That(result.pageItems[0].Id, Is.EqualTo(1));
            Assert.That(result.pageItems[19].Id, Is.EqualTo(20));
        }

        [Test]
        public void TestGetAuctionProductsPage_SecondPage_ReturnsCorrectItems()
        {
            // Arrange
            int currentPage = 2;
            
            // Act
            var result = auctionProductListViewModelHelper.GetAuctionProductsPage(
                auctionProductsViewModel,
                sortAndFilterViewModel,
                currentPage,
                ITEMS_PER_PAGE);
            
            // Assert
            Assert.That(result.pageItems, Is.Not.Null);
            Assert.That(result.pageItems.Count, Is.EqualTo(10)); // Remaining 10 items
            Assert.That(result.totalPages, Is.EqualTo(2));
            Assert.That(result.fullList.Count, Is.EqualTo(30));
            Assert.That(result.pageItems[0].Id, Is.EqualTo(21));
            Assert.That(result.pageItems[9].Id, Is.EqualTo(30));
        }

        [Test]
        public void TestGetAuctionProductsPage_EmptyList_ReturnsEmptyPage()
        {
            // Arrange
            int currentPage = 1;
            sortAndFilterViewModel = AuctionProductListViewModelHelperMocks.CreateSortAndFilterViewModel(
                new List<Product>());
            
            // Act
            var result = auctionProductListViewModelHelper.GetAuctionProductsPage(
                auctionProductsViewModel,
                sortAndFilterViewModel,
                currentPage,
                ITEMS_PER_PAGE);
            
            // Assert
            Assert.That(result.pageItems, Is.Not.Null);
            Assert.That(result.pageItems.Count, Is.EqualTo(0));
            Assert.That(result.totalPages, Is.EqualTo(0));
            Assert.That(result.fullList.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestShouldShowEmptyMessage_EmptyList_ReturnsTrue()
        {
            // Arrange
            var emptyList = new List<AuctionProduct>();
            
            // Act
            var result = auctionProductListViewModelHelper.ShouldShowEmptyMessage(emptyList);
            
            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestShouldShowEmptyMessage_NonEmptyList_ReturnsFalse()
        {
            // Arrange
            var nonEmptyList = AuctionProductListViewModelHelperMocks.CreateTestAuctionProducts(5);
            
            // Act
            var result = auctionProductListViewModelHelper.ShouldShowEmptyMessage(nonEmptyList);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestGetPaginationButtonState_FirstPage_ReturnsFalseForPrevious()
        {
            // Arrange
            int currentPage = BASE_PAGE;
            int totalPages = 5;
            
            // Act
            var (canGoToPrevious, canGoToNext) = auctionProductListViewModelHelper.GetPaginationButtonState(
                currentPage, totalPages, BASE_PAGE);
            
            // Assert
            Assert.That(canGoToPrevious, Is.False);
            Assert.That(canGoToNext, Is.True);
        }

        [Test]
        public void TestGetPaginationButtonState_MiddlePage_ReturnsTrueForBoth()
        {
            // Arrange
            int currentPage = 3;
            int totalPages = 5;
            
            // Act
            var (canGoToPrevious, canGoToNext) = auctionProductListViewModelHelper.GetPaginationButtonState(
                currentPage, totalPages, BASE_PAGE);
            
            // Assert
            Assert.That(canGoToPrevious, Is.True);
            Assert.That(canGoToNext, Is.True);
        }

        [Test]
        public void TestGetPaginationButtonState_LastPage_ReturnsFalseForNext()
        {
            // Arrange
            int currentPage = 5;
            int totalPages = 5;
            
            // Act
            var (canGoToPrevious, canGoToNext) = auctionProductListViewModelHelper.GetPaginationButtonState(
                currentPage, totalPages, BASE_PAGE);
            
            // Assert
            Assert.That(canGoToPrevious, Is.True);
            Assert.That(canGoToNext, Is.False);
        }

        [Test]
        public void TestGetPaginationText_WithPages_ReturnsCorrectText()
        {
            // Arrange
            int currentPage = 2;
            int totalPages = 5;
            
            // Act
            var result = auctionProductListViewModelHelper.GetPaginationText(currentPage, totalPages);
            
            // Assert
            Assert.That(result, Is.EqualTo("Page 2 of 5"));
        }

        [Test]
        public void TestGetPaginationText_ZeroPages_HandlesGracefully()
        {
            // Arrange
            int currentPage = 1;
            int totalPages = 0;
            
            // Act
            var result = auctionProductListViewModelHelper.GetPaginationText(currentPage, totalPages);
            
            // Assert
            Assert.That(result, Is.EqualTo("Page 1 of 1"));
        }
    }
} 