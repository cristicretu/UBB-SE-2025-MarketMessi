using DomainLayer.Domain;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketMinds.Test.Services.ProductTagService
{
    [TestFixture]
    internal class ProductServiceTests
    {
        // Constants to replace magic strings and numbers
        private const int ProductId1 = 1;
        private const int ProductId2 = 2;
        private const int ProductId3 = 3;
        private const int ProductId4 = 4;
        private const int CategoryId1 = 1;
        private const int CategoryId2 = 2;
        private const int ConditionId1 = 1;
        private const int ConditionId2 = 2;
        private const int TagId1 = 1;
        private const int TagId2 = 2;

        // Product titles
        private const string ProductTitleLaptop = "Laptop";
        private const string ProductTitleSmartphone = "Smartphone";
        private const string ProductTitleTablet = "Tablet";
        private const string ProductTitleHeadphones = "Headphones";
        private const string ProductTitleKeyboard = "Keyboard";
        private const string ProductTitleProduct1 = "Product1";
        private const string ProductTitleProduct2 = "Product2";
        private const string ProductTitleProduct3 = "Product3";
        private const string ProductTitleProductA = "Product A";
        private const string ProductTitleProductB = "Product B";
        private const string ProductTitleProductC = "Product C";
        private const string ProductTitleGamingComputer = "Gaming Computer";
        private const string ProductTitleWirelessMouse = "Wireless Mouse";
        private const string ProductTitleWiredKeyboard = "Wired Keyboard";
        private const string ProductTitleUsedLaptop = "Used Laptop";
        private const string ProductTitleOfficeKeyboard = "Office Keyboard";
        private const string ProductTitleOfficeChair = "Office Chair";
        private const string ProductTitleGamingMouse = "Gaming Mouse";
        private const string ProductTitleGamingLaptop = "Gaming Laptop";
        private const string ProductTitleBusinessDesktop = "Business Desktop";
        private const string ProductTitleGraphicTablet = "Graphic Tablet";
        private const string ProductTitleInitialName = "Initial Name";
        private const string ProductTitleUpdatedName = "Updated Name";
        private const string ProductTitleLaptopComputer = "LAPTOP Computer";
        private const string ProductTitleDesktopComputer = "Desktop computer";
        private const string ProductTitleProduct1WithCategory2 = "Product 1";
        private const string ProductTitleProduct2WithCategory1 = "Product 2";
        private const string ProductTitleProduct3WithCategory1 = "Product 3";

        // Category titles
        private const string CategoryElectronicsTitle = "Electronics";
        private const string CategoryElectronicsDesc = "Electronic devices";
        private const string CategoryClothingTitle = "Clothing";
        private const string CategoryClothingDesc = "Apparel";
        private const string CategoryDefaultTitle = "Default Category";
        private const string CategoryDefaultDesc = "Default category description";
        private const string CategoryACategoryTitle = "A Category";
        private const string CategoryACategoryDesc = "First category";
        private const string CategoryBCategoryTitle = "B Category";
        private const string CategoryBCategoryDesc = "Second category";

        // Condition titles
        private const string ConditionNewTitle = "New";
        private const string ConditionNewDesc = "Brand new";
        private const string ConditionUsedTitle = "Used";
        private const string ConditionUsedDesc = "Used item";
        private const string ConditionDefaultTitle = "New";
        private const string ConditionDefaultDesc = "Unused product in original packaging";

        // Tag titles
        private const string TagWirelessTitle = "Wireless";
        private const string TagBluetoothTitle = "Bluetooth";
        private const string TagOfficeTitle = "Office";
        private const string TagGamingTitle = "Gaming";

        // Search queries
        private const string SearchQueryComputer = "computer";
        private const string SearchQueryWireless = "wireless";
        private const string SearchQueryTop = "top";

        // Sort field names
        private const string SortFieldIdExternalName = "Id";
        private const string SortFieldIdInternalName = "Id";
        private const string SortFieldTitleExternalName = "Title";
        private const string SortFieldTitleInternalName = "Title";
        private const string SortFieldCategoryExternalName = "CategoryName";
        private const string SortFieldCategoryInternalName = "Category.DisplayTitle";

        // Common test values
        private const string DefaultProductDescription = "Test product description";
        private const int ExpectedItemCount0 = 0;
        private const int ExpectedItemCount1 = 1;
        private const int ExpectedItemCount2 = 2;
        private const int ExpectedItemCount3 = 3;

        private class TestProduct : Product
        {
        }

        #region Helper Methods

        private ProductRepositoryMock CreateMockRepository()
        {
            return new ProductRepositoryMock();
        }

        private MarketMinds.Services.ProductTagService.ProductService CreateProductService(ProductRepositoryMock repository)
        {
            return new MarketMinds.Services.ProductTagService.ProductService(repository);
        }

        private Product CreateSampleProduct(int id, string title, ProductCategory category = null, ProductCondition condition = null)
        {
            return new TestProduct
            {
                Id = id,
                Title = title,
                Description = DefaultProductDescription,
                Category = category ?? new ProductCategory(CategoryId1, CategoryDefaultTitle, CategoryDefaultDesc),
                Condition = condition ?? new ProductCondition(ConditionId1, ConditionDefaultTitle, ConditionDefaultDesc),
                Tags = new List<ProductTag>(),
                Images = new List<Image>()
            };
        }

        #endregion

        #region GetProducts Tests

        [Test]
        public void GetProducts_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            AddThreeSampleProducts(mockRepository);

            // Act
            var result = productService.GetProducts();

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount3));
        }

        [Test]
        public void GetProducts_ContainsFirstProduct()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            AddThreeSampleProducts(mockRepository);

            // Act
            var result = productService.GetProducts();

            // Assert
            Assert.That(result.Any(p => p.Id == ProductId1 && p.Title == ProductTitleLaptop), Is.True);
        }

        [Test]
        public void GetProducts_ContainsSecondProduct()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            AddThreeSampleProducts(mockRepository);

            // Act
            var result = productService.GetProducts();

            // Assert
            Assert.That(result.Any(p => p.Id == ProductId2 && p.Title == ProductTitleSmartphone), Is.True);
        }

        [Test]
        public void GetProducts_ContainsThirdProduct()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            AddThreeSampleProducts(mockRepository);

            // Act
            var result = productService.GetProducts();

            // Assert
            Assert.That(result.Any(p => p.Id == ProductId3 && p.Title == ProductTitleTablet), Is.True);
        }

        private void AddThreeSampleProducts(ProductRepositoryMock mockRepository)
        {
            var product1 = CreateSampleProduct(ProductId1, ProductTitleLaptop);
            var product2 = CreateSampleProduct(ProductId2, ProductTitleSmartphone);
            var product3 = CreateSampleProduct(ProductId3, ProductTitleTablet);

            mockRepository.AddProduct(product1);
            mockRepository.AddProduct(product2);
            mockRepository.AddProduct(product3);
        }

        #endregion

        #region GetProductById Tests

        [Test]
        public void GetProductById_ReturnsNonNullProduct()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            AddTwoSampleProducts(mockRepository);

            // Act
            var result = productService.GetProductById(ProductId2);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetProductById_ReturnsProductWithCorrectId()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            AddTwoSampleProducts(mockRepository);

            // Act
            var result = productService.GetProductById(ProductId2);

            // Assert
            Assert.That(result.Id, Is.EqualTo(ProductId2));
        }

        [Test]
        public void GetProductById_ReturnsProductWithCorrectTitle()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            AddTwoSampleProducts(mockRepository);

            // Act
            var result = productService.GetProductById(ProductId2);

            // Assert
            Assert.That(result.Title, Is.EqualTo(ProductTitleSmartphone));
        }

        private void AddTwoSampleProducts(ProductRepositoryMock mockRepository)
        {
            var product1 = CreateSampleProduct(ProductId1, ProductTitleLaptop);
            var product2 = CreateSampleProduct(ProductId2, ProductTitleSmartphone);

            mockRepository.AddProduct(product1);
            mockRepository.AddProduct(product2);
        }

        #endregion

        #region AddProduct Tests

        [Test]
        public void AddProduct_AddsProductToRepository()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            var product = CreateSampleProduct(ProductId1, ProductTitleHeadphones);

            // Act
            productService.AddProduct(product);

            // Assert
            Assert.That(mockRepository.Products.Count, Is.EqualTo(ExpectedItemCount1));
        }

        [Test]
        public void AddProduct_AddsProductWithCorrectId()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            var product = CreateSampleProduct(ProductId1, ProductTitleHeadphones);

            // Act
            productService.AddProduct(product);

            // Assert
            Assert.That(mockRepository.Products[0].Id, Is.EqualTo(ProductId1));
        }

        [Test]
        public void AddProduct_AddsProductWithCorrectTitle()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            var product = CreateSampleProduct(ProductId1, ProductTitleHeadphones);

            // Act
            productService.AddProduct(product);

            // Assert
            Assert.That(mockRepository.Products[0].Title, Is.EqualTo(ProductTitleHeadphones));
        }

        #endregion

        #region DeleteProduct Tests

        [Test]
        public void DeleteProduct_RemovesProductFromRepository()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);
            var product = CreateSampleProduct(ProductId1, ProductTitleKeyboard);

            mockRepository.AddProduct(product);
            Assert.That(mockRepository.Products.Count, Is.EqualTo(ExpectedItemCount1),
                "Precondition: Repository should have one product before deletion");

            // Act
            productService.DeleteProduct(product);

            // Assert
            Assert.That(mockRepository.Products.Count, Is.EqualTo(ExpectedItemCount0));
        }

        #endregion

        #region UpdateProduct Tests

        [Test]
        public void UpdateProduct_UpdatesProductInRepository()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);

            var productToUpdate = CreateSampleProduct(ProductId1, ProductTitleInitialName);
            mockRepository.AddProduct(productToUpdate);
            productToUpdate.Title = ProductTitleUpdatedName;

            // Act
            productService.UpdateProduct(productToUpdate);

            // Assert
            var updatedProduct = mockRepository.Products.FirstOrDefault(p => p.Id == ProductId1);
            Assert.That(updatedProduct, Is.Not.Null);
            Assert.That(updatedProduct.Title, Is.EqualTo(ProductTitleUpdatedName));
        }

        [Test]
        public void UpdateProduct_DoesNotAddMoreProducts()
        {
            // Arrange
            var mockRepository = CreateMockRepository();
            var productService = CreateProductService(mockRepository);

            var productToUpdate = CreateSampleProduct(ProductId1, ProductTitleInitialName);
            mockRepository.AddProduct(productToUpdate);
            productToUpdate.Title = ProductTitleUpdatedName;

            // Act
            productService.UpdateProduct(productToUpdate);

            // Assert
            Assert.That(mockRepository.Products.Count, Is.EqualTo(ExpectedItemCount1));
        }

        #endregion

        #region GetSortedFilteredProducts - Basic Tests

        [Test]
        public void GetSortedFilteredProducts_EmptyProductList_ReturnsEmptyList()
        {
            // Arrange
            var products = new List<Product>();

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, null, null, null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount0));
        }

        [Test]
        public void GetSortedFilteredProducts_WithNullFirstParameter_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                    null, null, null, null, null, null));
        }

        #endregion

        #region GetSortedFilteredProducts - Filtering by Condition Tests

        [Test]
        public void GetSortedFilteredProducts_WithMultipleConditions_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            var condition1 = new ProductCondition(ConditionId1, ConditionNewTitle, ConditionNewDesc);
            var condition2 = new ProductCondition(ConditionId2, ConditionUsedTitle, ConditionUsedDesc);
            var products = CreateProductsWithDifferentConditions(condition1, condition2);
            var selectedConditions = new List<ProductCondition> { condition1, condition2 };

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, selectedConditions, null, null, null, null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount3));
        }

        private List<Product> CreateProductsWithDifferentConditions(
            ProductCondition condition1, ProductCondition condition2)
        {
            var product1 = CreateSampleProduct(ProductId1, ProductTitleProduct1, condition: condition1);
            var product2 = CreateSampleProduct(ProductId2, ProductTitleProduct2, condition: condition2);
            var product3 = CreateSampleProduct(ProductId3, ProductTitleProduct3, condition: condition1);

            return new List<Product> { product1, product2, product3 };
        }

        #endregion

        #region GetSortedFilteredProducts - Filtering by Category Tests

        [Test]
        public void GetSortedFilteredProducts_WithMultipleCategories_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            var category1 = new ProductCategory(CategoryId1, CategoryElectronicsTitle, CategoryElectronicsDesc);
            var category2 = new ProductCategory(CategoryId2, CategoryClothingTitle, CategoryClothingDesc);
            var products = CreateProductsWithDifferentCategories(category1, category2);
            var selectedCategories = new List<ProductCategory> { category1, category2 };

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, selectedCategories, null, null, null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount3));
        }

        private List<Product> CreateProductsWithDifferentCategories(
            ProductCategory category1, ProductCategory category2)
        {
            var product1 = CreateSampleProduct(ProductId1, ProductTitleProduct1, category: category1);
            var product2 = CreateSampleProduct(ProductId2, ProductTitleProduct2, category: category2);
            var product3 = CreateSampleProduct(ProductId3, ProductTitleProduct3, category: category1);

            return new List<Product> { product1, product2, product3 };
        }

        #endregion

        #region GetSortedFilteredProducts - Filtering by Tag Tests

        [Test]
        public void GetSortedFilteredProducts_WithMultipleTags_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            var tag1 = new ProductTag(TagId1, TagWirelessTitle);
            var tag2 = new ProductTag(TagId2, TagBluetoothTitle);
            var products = CreateProductsWithDifferentTags(tag1, tag2);
            var selectedTags = new List<ProductTag> { tag1, tag2 };

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, selectedTags, null, null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount3));
        }

        private List<Product> CreateProductsWithDifferentTags(ProductTag tag1, ProductTag tag2)
        {
            var product1 = CreateSampleProduct(ProductId1, ProductTitleProduct1);
            product1.Tags.Add(tag1);

            var product2 = CreateSampleProduct(ProductId2, ProductTitleProduct2);
            product2.Tags.Add(tag2);

            var product3 = CreateSampleProduct(ProductId3, ProductTitleProduct3);
            product3.Tags.Add(tag1);
            product3.Tags.Add(tag2);

            return new List<Product> { product1, product2, product3 };
        }

        #endregion

        #region GetSortedFilteredProducts - Text Search Tests

        [Test]
        public void GetSortedFilteredProducts_WithCaseInsensitiveSearch_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            var products = CreateProductsForTextSearch();

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, null, null, SearchQueryComputer);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount2));
        }

        [Test]
        public void GetSortedFilteredProducts_WithCaseInsensitiveSearch_ReturnsProductsContainingSearchTerm()
        {
            // Arrange
            var products = CreateProductsForTextSearch();

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, null, null, SearchQueryComputer);

            // Assert
            Assert.That(result.All(p => p.Title.ToLower().Contains(SearchQueryComputer)), Is.True);
        }

        [Test]
        public void GetSortedFilteredProducts_WithPartialWordSearch_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateSampleProduct(ProductId1, ProductTitleGamingLaptop),
                CreateSampleProduct(ProductId2, ProductTitleBusinessDesktop),
                CreateSampleProduct(ProductId3, ProductTitleGraphicTablet)
            };

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, null, null, SearchQueryTop);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount2));
        }

        [Test]
        public void GetSortedFilteredProducts_WithPartialWordSearch_ReturnsProductsContainingSearchTerm()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateSampleProduct(ProductId1, ProductTitleGamingLaptop),
                CreateSampleProduct(ProductId2, ProductTitleBusinessDesktop),
                CreateSampleProduct(ProductId3, ProductTitleGraphicTablet)
            };

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, null, null, SearchQueryTop);

            // Assert
            Assert.That(result.All(p => p.Title.ToLower().Contains(SearchQueryTop)), Is.True);
        }

        private List<Product> CreateProductsForTextSearch()
        {
            return new List<Product>
            {
                CreateSampleProduct(ProductId1, ProductTitleLaptopComputer),
                CreateSampleProduct(ProductId2, ProductTitleDesktopComputer),
                CreateSampleProduct(ProductId3, ProductTitleTablet)
            };
        }

        #endregion

        #region GetSortedFilteredProducts - Combined Filtering Tests

        [Test]
        public void GetSortedFilteredProducts_WithAllFilterTypes_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            var (category, condition, tag, products) = SetupAllFilterTypesTest();

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products,
                new List<ProductCondition> { condition },
                new List<ProductCategory> { category },
                new List<ProductTag> { tag },
                null,
                SearchQueryWireless
            );

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount1));
        }

        [Test]
        public void GetSortedFilteredProducts_WithAllFilterTypes_ReturnsCorrectProduct()
        {
            // Arrange
            var (category, condition, tag, products) = SetupAllFilterTypesTest();

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products,
                new List<ProductCondition> { condition },
                new List<ProductCategory> { category },
                new List<ProductTag> { tag },
                null,
                SearchQueryWireless
            );

            // Assert
            Assert.That(result[0].Title, Is.EqualTo(ProductTitleWirelessMouse));
        }

        private (ProductCategory, ProductCondition, ProductTag, List<Product>) SetupAllFilterTypesTest()
        {
            var category = new ProductCategory(CategoryId1, CategoryElectronicsTitle, CategoryElectronicsDesc);
            var condition = new ProductCondition(ConditionId1, ConditionNewTitle, ConditionNewDesc);
            var tag = new ProductTag(TagId1, TagWirelessTitle);

            var product1 = CreateSampleProduct(ProductId1, ProductTitleGamingComputer, category, condition);
            product1.Tags.Add(tag);

            var product2 = CreateSampleProduct(ProductId2, ProductTitleWirelessMouse, category, condition);
            product2.Tags.Add(tag);

            var product3 = CreateSampleProduct(ProductId3, ProductTitleWiredKeyboard, category, condition);

            var product4 = CreateSampleProduct(ProductId4, ProductTitleUsedLaptop, category,
                new ProductCondition(ConditionId2, ConditionUsedTitle, ConditionUsedDesc));
            product4.Tags.Add(tag);

            var products = new List<Product> { product1, product2, product3, product4 };

            return (category, condition, tag, products);
        }

        [Test]
        public void GetSortedFilteredProducts_WithFilteringAndNullSort_FiltersCorrectly()
        {
            // Arrange
            var condition = new ProductCondition(ConditionId1, ConditionNewTitle, ConditionNewDesc);
            var products = CreateProductsForFilteringTest(condition);
            var selectedConditions = new List<ProductCondition> { condition };

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, selectedConditions, null, null, null, null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount2));
        }

        [Test]
        public void GetSortedFilteredProducts_WithFilteringAndNullSort_ReturnsProductsWithMatchingCondition()
        {
            // Arrange
            var condition = new ProductCondition(ConditionId1, ConditionNewTitle, ConditionNewDesc);
            var products = CreateProductsForFilteringTest(condition);
            var selectedConditions = new List<ProductCondition> { condition };

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, selectedConditions, null, null, null, null);

            // Assert
            Assert.That(result.All(p => p.Condition.Id == condition.Id), Is.True);
        }

        private List<Product> CreateProductsForFilteringTest(ProductCondition condition)
        {
            var product1 = CreateSampleProduct(ProductId1, ProductTitleProductA, condition: condition);
            var product2 = CreateSampleProduct(ProductId2, ProductTitleProductB,
                condition: new ProductCondition(ConditionId2, ConditionUsedTitle, ConditionUsedDesc));
            var product3 = CreateSampleProduct(ProductId3, ProductTitleProductC, condition: condition);

            return new List<Product> { product1, product2, product3 };
        }

        #endregion

        #region GetSortedFilteredProducts - Sorting Tests

        [Test]
        public void GetSortedFilteredProducts_WithSortingId_SortsProductsCorrectly()
        {
            // Arrange
            var products = CreateProductListForSorting();
            var sortCondition = new ProductSortType(
                SortFieldIdExternalName, SortFieldIdInternalName, true);

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, null, sortCondition, null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount3));
            Assert.That(result[0].Id, Is.EqualTo(ProductId1));
            Assert.That(result[1].Id, Is.EqualTo(ProductId2));
            Assert.That(result[2].Id, Is.EqualTo(ProductId3));
        }

        [Test]
        public void GetSortedFilteredProducts_WithSortingDescendingId_SortsProductsCorrectly()
        {
            // Arrange
            var products = CreateProductListForSorting();
            var sortCondition = new ProductSortType(
                SortFieldIdExternalName, SortFieldIdInternalName, false);

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, null, sortCondition, null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount3));
            Assert.That(result[0].Id, Is.EqualTo(ProductId3));
            Assert.That(result[1].Id, Is.EqualTo(ProductId2));
            Assert.That(result[2].Id, Is.EqualTo(ProductId1));
        }

        private List<Product> CreateProductListForSorting()
        {
            return new List<Product>
            {
                CreateSampleProduct(ProductId1, ProductTitleProductA),
                CreateSampleProduct(ProductId2, ProductTitleProductB),
                CreateSampleProduct(ProductId3, ProductTitleProductC)
            };
        }

        [Test]
        public void GetSortedFilteredProducts_WithSortingAndFiltering_FiltersThenSortsCorrectly()
        {
            // Arrange
            var products = SetupSortingAndFilteringTest(out var tag1, out var selectedTags);
            var sortCondition = new ProductSortType(
                SortFieldTitleExternalName, SortFieldTitleInternalName, true);

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, selectedTags, sortCondition, null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount2));
            Assert.That(result[0].Title, Is.EqualTo(ProductTitleOfficeChair));
            Assert.That(result[1].Title, Is.EqualTo(ProductTitleOfficeKeyboard));
        }

        private List<Product> SetupSortingAndFilteringTest(
            out ProductTag tag1, out List<ProductTag> selectedTags)
        {
            tag1 = new ProductTag(TagId1, TagOfficeTitle);
            var tag2 = new ProductTag(TagId2, TagGamingTitle);

            var product1 = CreateSampleProduct(ProductId1, ProductTitleOfficeKeyboard);
            product1.Tags.Add(tag1);

            var product2 = CreateSampleProduct(ProductId2, ProductTitleOfficeChair);
            product2.Tags.Add(tag1);

            var product3 = CreateSampleProduct(ProductId3, ProductTitleGamingMouse);
            product3.Tags.Add(tag2);

            selectedTags = new List<ProductTag> { tag1 };

            return new List<Product> { product1, product2, product3 };
        }

        [Test]
        public void GetSortedFilteredProducts_WithComplexSorting_HandlesComplexProperties()
        {
            // Arrange
            var category1 = new ProductCategory(CategoryId1, CategoryACategoryTitle, CategoryACategoryDesc);
            var category2 = new ProductCategory(CategoryId2, CategoryBCategoryTitle, CategoryBCategoryDesc);

            var product1 = CreateSampleProduct(ProductId1, ProductTitleProduct1WithCategory2, category2);
            var product2 = CreateSampleProduct(ProductId2, ProductTitleProduct2WithCategory1, category1);
            var product3 = CreateSampleProduct(ProductId3, ProductTitleProduct3WithCategory1, category1);

            var products = new List<Product> { product1, product2, product3 };
            var sortCondition = new ProductSortType(
                SortFieldCategoryExternalName, SortFieldCategoryInternalName, true);

            // Act
            var result = MarketMinds.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
                products, null, null, null, sortCondition, null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedItemCount3));
        }

        #endregion
    }
}

