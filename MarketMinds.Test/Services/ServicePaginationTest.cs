using System;
using System.Collections.Generic;
using System.Linq;
using MarketMinds.Shared.Models;
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
        private const int ITEMS_PER_PAGE = 3;
        private const int DEFAULT_REQUEST_PAGE = 1;
        private const int SECOND_REQUEST_PAGE = 2;
        private const int NON_EXISTENT_PAGE = 10;
        private const int INVALID_PAGE_ZERO = 0;
        private const int EXPECTED_TOTAL_PAGES_NORMAL_CASE = 3;
        private const int EXPECTED_ITEM_COUNT_NORMAL_CASE = 3;
        private const int EXPECTED_TOTAL_PAGES_EXCEED_CASE = 2;
        private const int EXPECTED_ITEM_COUNT_EXCEED_CASE = 2;

        // Constants for parameter names in exceptions
        private const string ALL_PRODUCTS_PARAM_NAME = "allProducts";
        private const string CURRENT_PAGE_PARAM_NAME = "currentPage";
        private const string PRODUCTS_PARAM_NAME = "products";
        private const string FILTER_PREDICATE_PARAM_NAME = "filterPredicate";

        // Constants for test data
        private static readonly List<int> NORMAL_CASE_PRODUCTS = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static readonly List<int> EXCEED_CASE_PRODUCTS = new List<int> { 10, 20, 30, 40, 50 };
        private static readonly List<int> MINIMAL_PRODUCTS = new List<int> { 1, 2, 3 };
        private static readonly List<int> FILTER_TEST_PRODUCTS = new List<int> { 1, 2, 3, 4, 5 };
        private static readonly List<int> EXPECTED_SECOND_PAGE = new List<int> { 4, 5, 6 };
        private static readonly List<int> EXPECTED_LAST_PAGE = new List<int> { 40, 50 };
        private static readonly List<int> EXPECTED_FILTERED_PRODUCTS = new List<int> { 2, 4 };

        private ProductPaginationService _paginationService;

        [SetUp]
        public void SetUp()
        {
            _paginationService = new ProductPaginationService(ITEMS_PER_PAGE);
        }

        #region GetPaginatedProducts Tests

        [Test]
        public void GetPaginatedProducts_NormalCase_ReturnsCorrectPageCount()
        {
            // Arrange
            List<int> products = NORMAL_CASE_PRODUCTS;

            // Act: Request page 2 (should return items 4,5,6)
            var (currentPage, totalPages) = _paginationService.GetPaginatedProducts(products, SECOND_REQUEST_PAGE);

            // Assert
            Assert.That(currentPage.Count, Is.EqualTo(EXPECTED_ITEM_COUNT_NORMAL_CASE));
        }

        [Test]
        public void GetPaginatedProducts_NormalCase_ReturnsCorrectPageItems()
        {
            // Arrange
            List<int> products = NORMAL_CASE_PRODUCTS;

            // Act: Request page 2 (should return items 4,5,6)
            var (currentPage, _) = _paginationService.GetPaginatedProducts(products, SECOND_REQUEST_PAGE);

            // Assert
            CollectionAssert.AreEqual(EXPECTED_SECOND_PAGE, currentPage);
        }

        [Test]
        public void GetPaginatedProducts_NormalCase_ReturnsCorrectTotalPages()
        {
            // Arrange
            List<int> products = NORMAL_CASE_PRODUCTS;

            // Act: Request page 2
            var (_, totalPages) = _paginationService.GetPaginatedProducts(products, SECOND_REQUEST_PAGE);

            // Assert
            // Total pages: 8 / 3 = 2.67, ceil -> 3 pages.
            Assert.That(totalPages, Is.EqualTo(EXPECTED_TOTAL_PAGES_NORMAL_CASE));
        }

        [Test]
        public void GetPaginatedProducts_CurrentPageExceedsTotalPages_ReturnsCorrectItemCount()
        {
            // Arrange
            List<int> products = EXCEED_CASE_PRODUCTS;

            // Act: Request page 10 which is greater than total pages (5/3 = 1.67, ceil -> 2)
            var (currentPage, _) = _paginationService.GetPaginatedProducts(products, NON_EXISTENT_PAGE);

            // Assert
            // Expected last page: page 2 should have 2 items (items 4 and 5)
            Assert.That(currentPage.Count, Is.EqualTo(EXPECTED_ITEM_COUNT_EXCEED_CASE));
        }

        [Test]
        public void GetPaginatedProducts_CurrentPageExceedsTotalPages_ReturnsCorrectItems()
        {
            // Arrange
            List<int> products = EXCEED_CASE_PRODUCTS;

            // Act: Request page 10 which is greater than total pages (5/3 = 1.67, ceil -> 2)
            var (currentPage, _) = _paginationService.GetPaginatedProducts(products, NON_EXISTENT_PAGE);

            // Assert
            CollectionAssert.AreEqual(EXPECTED_LAST_PAGE, currentPage);
        }

        [Test]
        public void GetPaginatedProducts_CurrentPageExceedsTotalPages_ReturnsCorrectTotalPages()
        {
            // Arrange
            List<int> products = EXCEED_CASE_PRODUCTS;

            // Act: Request page 10 which is greater than total pages (5/3 = 1.67, ceil -> 2)
            var (_, totalPages) = _paginationService.GetPaginatedProducts(products, NON_EXISTENT_PAGE);

            // Assert
            Assert.That(totalPages, Is.EqualTo(EXPECTED_TOTAL_PAGES_EXCEED_CASE));
        }

        [Test]
        public void GetPaginatedProducts_AllProductsIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            List<int> products = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                _paginationService.GetPaginatedProducts(products, DEFAULT_REQUEST_PAGE));

            Assert.That(ex.ParamName, Is.EqualTo(ALL_PRODUCTS_PARAM_NAME));
        }

        [Test]
        public void GetPaginatedProducts_CurrentPageLessThanBase_ThrowsArgumentException()
        {
            // Arrange
            List<int> products = MINIMAL_PRODUCTS;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _paginationService.GetPaginatedProducts(products, INVALID_PAGE_ZERO));

            Assert.That(ex.ParamName, Is.EqualTo(CURRENT_PAGE_PARAM_NAME));
        }

        #endregion

        #region ApplyFilters Tests

        [Test]
        public void ApplyFilters_NormalCase_ReturnsFilteredProducts()
        {
            // Arrange
            List<int> products = FILTER_TEST_PRODUCTS;
            // Filter to return only even numbers.
            Func<int, bool> evenFilter = p => p % 2 == 0;

            // Act
            var filteredProducts = _paginationService.ApplyFilters(products, evenFilter);

            // Assert
            CollectionAssert.AreEqual(EXPECTED_FILTERED_PRODUCTS, filteredProducts);
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

            Assert.That(ex.ParamName, Is.EqualTo(PRODUCTS_PARAM_NAME));
        }

        [Test]
        public void ApplyFilters_PredicateIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            List<int> products = MINIMAL_PRODUCTS;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                _paginationService.ApplyFilters(products, null));

            Assert.That(ex.ParamName, Is.EqualTo(FILTER_PREDICATE_PARAM_NAME));
        }

        #endregion
    }
}
