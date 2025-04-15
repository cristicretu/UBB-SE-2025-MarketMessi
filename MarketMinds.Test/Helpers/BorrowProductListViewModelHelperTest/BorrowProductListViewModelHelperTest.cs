using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DomainLayer.Domain;
using ViewModelLayer.ViewModel;
using BusinessLogicLayer.ViewModel;
using MarketMinds.Helpers;
using MarketMinds.Services.ProductTagService;
using MarketMinds.Services.BorrowProductsService;
using Moq;

namespace MarketMinds.Test.Helpers.BorrowProductListViewModelHelperTest
{
    [TestFixture]
    public class BorrowProductListViewModelHelperTest
    {
        private BorrowProductListViewModelHelper borrowProductListViewModelHelper;
        private BorrowProductsViewModel borrowProductsViewModel;
        private SortAndFilterViewModel sortAndFilterViewModel;
        private List<BorrowProduct> testProducts;

        [SetUp]
        public void Setup()
        {
            borrowProductListViewModelHelper = new BorrowProductListViewModelHelper();
            
            // Create 30 test products (more than ItemsPerPage = 20)
            testProducts = BorrowProductListViewModelHelperMocks.CreateTestBorrowProducts(30);
            
            // Create SortAndFilterViewModel with a mocked product service
            sortAndFilterViewModel = BorrowProductListViewModelHelperMocks.CreateSortAndFilterViewModel(
                testProducts.Cast<Product>().ToList());
                
            // Create a real BorrowProductsViewModel or a simple mock that doesn't require mocking
            // Since BorrowProductsViewModel doesn't seem to be used in the helper methods we're testing
            var mockBorrowProductsService = new Mock<BorrowProductsService>(null);
            borrowProductsViewModel = new BorrowProductsViewModel(mockBorrowProductsService.Object);
        }

        [Test]
        public void TestGetBorrowProductsPage_FirstPage_ReturnsCorrectItems()
        {
            // Arrange
            int currentPage = 1;
            
            // Act
            var result = borrowProductListViewModelHelper.GetBorrowProductsPage(
                borrowProductsViewModel,
                sortAndFilterViewModel,
                currentPage);
            
            // Assert
            Assert.That(result.pageItems, Is.Not.Null);
            Assert.That(result.pageItems.Count, Is.EqualTo(20)); // First 20 items
            Assert.That(result.totalPages, Is.EqualTo(2)); // 30 items / 20 per page = 2 pages
            Assert.That(result.fullList.Count, Is.EqualTo(30));
            Assert.That(result.pageItems[0].Id, Is.EqualTo(1));
            Assert.That(result.pageItems[19].Id, Is.EqualTo(20));
        }

        [Test]
        public void TestGetBorrowProductsPage_SecondPage_ReturnsCorrectItems()
        {
            // Arrange
            int currentPage = 2;
            
            // Act
            var result = borrowProductListViewModelHelper.GetBorrowProductsPage(
                borrowProductsViewModel,
                sortAndFilterViewModel,
                currentPage);
            
            // Assert
            Assert.That(result.pageItems, Is.Not.Null);
            Assert.That(result.pageItems.Count, Is.EqualTo(10)); // Remaining 10 items
            Assert.That(result.totalPages, Is.EqualTo(2));
            Assert.That(result.fullList.Count, Is.EqualTo(30));
            Assert.That(result.pageItems[0].Id, Is.EqualTo(21));
            Assert.That(result.pageItems[9].Id, Is.EqualTo(30));
        }

        [Test]
        public void TestGetBorrowProductsPage_EmptyList_ReturnsEmptyPage()
        {
            // Arrange
            int currentPage = 1;
            sortAndFilterViewModel = BorrowProductListViewModelHelperMocks.CreateSortAndFilterViewModel(
                new List<Product>());
            
            // Act
            var result = borrowProductListViewModelHelper.GetBorrowProductsPage(
                borrowProductsViewModel,
                sortAndFilterViewModel,
                currentPage);
            
            // Assert
            Assert.That(result.pageItems, Is.Not.Null);
            Assert.That(result.pageItems.Count, Is.EqualTo(0));
            Assert.That(result.totalPages, Is.EqualTo(0));
            Assert.That(result.fullList.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestGetPaginationState_FirstPage_ReturnsCorrectState()
        {
            // Arrange
            int currentPage = 1;
            int totalPages = 5;
            
            // Act
            var result = borrowProductListViewModelHelper.GetPaginationState(currentPage, totalPages);
            
            // Assert
            Assert.That(result.hasPrevious, Is.False);
            Assert.That(result.hasNext, Is.True);
        }

        [Test]
        public void TestGetPaginationState_MiddlePage_ReturnsCorrectState()
        {
            // Arrange
            int currentPage = 3;
            int totalPages = 5;
            
            // Act
            var result = borrowProductListViewModelHelper.GetPaginationState(currentPage, totalPages);
            
            // Assert
            Assert.That(result.hasPrevious, Is.True);
            Assert.That(result.hasNext, Is.True);
        }

        [Test]
        public void TestGetPaginationState_LastPage_ReturnsCorrectState()
        {
            // Arrange
            int currentPage = 5;
            int totalPages = 5;
            
            // Act
            var result = borrowProductListViewModelHelper.GetPaginationState(currentPage, totalPages);
            
            // Assert
            Assert.That(result.hasPrevious, Is.True);
            Assert.That(result.hasNext, Is.False);
        }

        [Test]
        public void TestGetPaginationState_SinglePage_ReturnsFalseForBoth()
        {
            // Arrange
            int currentPage = 1;
            int totalPages = 1;
            
            // Act
            var result = borrowProductListViewModelHelper.GetPaginationState(currentPage, totalPages);
            
            // Assert
            Assert.That(result.hasPrevious, Is.False);
            Assert.That(result.hasNext, Is.False);
        }

        [Test]
        public void TestGetPaginationText_WithPages_ReturnsCorrectText()
        {
            // Arrange
            int currentPage = 2;
            int totalPages = 5;
            
            // Act
            var result = borrowProductListViewModelHelper.GetPaginationText(currentPage, totalPages);
            
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
            var result = borrowProductListViewModelHelper.GetPaginationText(currentPage, totalPages);
            
            // Assert
            Assert.That(result, Is.EqualTo("Page 1 of 1"));
        }
    }
} 