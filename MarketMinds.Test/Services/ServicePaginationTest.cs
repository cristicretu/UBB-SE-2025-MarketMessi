using System;
using System.Collections.Generic;
using System.Linq;
using DomainLayer.Domain;
using MarketMinds.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using MarketMinds.Services.ProductPaginationService;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    internal class ProductPaginationServiceTest
    {
        // Constants for test configuration
        private const int ItemsPerPage = 3;
        private const int DefaultRequestPage = 1;
        private const int SecondRequestPage = 2;
        private const int NonExistentPage = 10;
        private const int InvalidPageZero = 0;
        private const int ExpectedTotalPagesNormalCase = 3;
        private const int ExpectedItemCountNormalCase = 3;
        private const int ExpectedTotalPagesExceedCase = 2;
        private const int ExpectedItemCountExceedCase = 2;

        // Constants for parameter names in exceptions
        private const string AllProductsParamName = "allProducts";
        private const string CurrentPageParamName = "currentPage";
        private const string ProductsParamName = "products";
        private const string FilterPredicateParamName = "filterPredicate";

        // Constants for test data
        private static readonly List<int> NormalCaseProducts = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static readonly List<int> ExceedCaseProducts = new List<int> { 10, 20, 30, 40, 50 };
        private static readonly List<int> MinimalProducts = new List<int> { 1, 2, 3 };
        private static readonly List<int> FilterTestProducts = new List<int> { 1, 2, 3, 4, 5 };
        private static readonly List<int> ExpectedSecondPage = new List<int> { 4, 5, 6 };
        private static readonly List<int> ExpectedLastPage = new List<int> { 40, 50 };
        private static readonly List<int> ExpectedFilteredProducts = new List<int> { 2, 4 };

        private ProductPaginationService _paginationService;

        [SetUp]
        public void SetUp()
        {
            _paginationService = new ProductPaginationService(ItemsPerPage);
        }

        #region GetPaginatedProducts Tests

        [Test]
        public void GetPaginatedProducts_NormalCase_ReturnsCorrectPageCount()
        {
            // Arrange
            List<int> products = NormalCaseProducts;

            // Act: Request page 2 (should return items 4,5,6)
            var (currentPage, totalPages) = _paginationService.GetPaginatedProducts(products, SecondRequestPage);

            // Assert
            Assert.That(currentPage.Count, Is.EqualTo(ExpectedItemCountNormalCase));
        }

        [Test]
        public void GetPaginatedProducts_NormalCase_ReturnsCorrectPageItems()
        {
            // Arrange
            List<int> products = NormalCaseProducts;

            // Act: Request page 2 (should return items 4,5,6)
            var (currentPage, _) = _paginationService.GetPaginatedProducts(products, SecondRequestPage);

            // Assert
            CollectionAssert.AreEqual(ExpectedSecondPage, currentPage);
        }

        [Test]
        public void GetPaginatedProducts_NormalCase_ReturnsCorrectTotalPages()
        {
            // Arrange
            List<int> products = NormalCaseProducts;

            // Act: Request page 2
            var (_, totalPages) = _paginationService.GetPaginatedProducts(products, SecondRequestPage);

            // Assert
            // Total pages: 8 / 3 = 2.67, ceil -> 3 pages.
            Assert.That(totalPages, Is.EqualTo(ExpectedTotalPagesNormalCase));
        }

        [Test]
        public void GetPaginatedProducts_CurrentPageExceedsTotalPages_ReturnsCorrectItemCount()
        {
            // Arrange
            List<int> products = ExceedCaseProducts;

            // Act: Request page 10 which is greater than total pages (5/3 = 1.67, ceil -> 2)
            var (currentPage, _) = _paginationService.GetPaginatedProducts(products, NonExistentPage);

            // Assert
            // Expected last page: page 2 should have 2 items (items 4 and 5)
            Assert.That(currentPage.Count, Is.EqualTo(ExpectedItemCountExceedCase));
        }

        [Test]
        public void GetPaginatedProducts_CurrentPageExceedsTotalPages_ReturnsCorrectItems()
        {
            // Arrange
            List<int> products = ExceedCaseProducts;

            // Act: Request page 10 which is greater than total pages (5/3 = 1.67, ceil -> 2)
            var (currentPage, _) = _paginationService.GetPaginatedProducts(products, NonExistentPage);

            // Assert
            CollectionAssert.AreEqual(ExpectedLastPage, currentPage);
        }

        [Test]
        public void GetPaginatedProducts_CurrentPageExceedsTotalPages_ReturnsCorrectTotalPages()
        {
            // Arrange
            List<int> products = ExceedCaseProducts;

            // Act: Request page 10 which is greater than total pages (5/3 = 1.67, ceil -> 2)
            var (_, totalPages) = _paginationService.GetPaginatedProducts(products, NonExistentPage);

            // Assert
            Assert.That(totalPages, Is.EqualTo(ExpectedTotalPagesExceedCase));
        }

        [Test]
        public void GetPaginatedProducts_AllProductsIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            List<int> products = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                _paginationService.GetPaginatedProducts(products, DefaultRequestPage));

            Assert.That(ex.ParamName, Is.EqualTo(AllProductsParamName));
        }

        [Test]
        public void GetPaginatedProducts_CurrentPageLessThanBase_ThrowsArgumentException()
        {
            // Arrange
            List<int> products = MinimalProducts;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _paginationService.GetPaginatedProducts(products, InvalidPageZero));

            Assert.That(ex.ParamName, Is.EqualTo(CurrentPageParamName));
        }

        #endregion

        #region ApplyFilters Tests

        [Test]
        public void ApplyFilters_NormalCase_ReturnsFilteredProducts()
        {
            // Arrange
            List<int> products = FilterTestProducts;
            // Filter to return only even numbers.
            Func<int, bool> evenFilter = p => p % 2 == 0;

            // Act
            var filteredProducts = _paginationService.ApplyFilters(products, evenFilter);

            // Assert
            CollectionAssert.AreEqual(ExpectedFilteredProducts, filteredProducts);
        }

        [Test]
        public void ApplyFilters_ProductsIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            List<int> products = null;
            Func<int, bool> predicate = p => true;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                _paginationService.ApplyFilters(products, predicate));

            Assert.That(ex.ParamName, Is.EqualTo(ProductsParamName));
        }

        [Test]
        public void ApplyFilters_PredicateIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            List<int> products = MinimalProducts;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                _paginationService.ApplyFilters(products, null));

            Assert.That(ex.ParamName, Is.EqualTo(FilterPredicateParamName));
        }

        #endregion
    }
}
