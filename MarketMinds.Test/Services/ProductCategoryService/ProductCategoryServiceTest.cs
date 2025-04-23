using DomainLayer.Domain;
using MarketMinds.Services.ProductCategoryService;
using NUnit.Framework;

namespace MarketMinds.Test.Services.ProductCategoryService
{
    [TestFixture]
    public class ProductCategoryServiceTests
    {
        // Constants to replace magic strings and numbers
        private const int FirstCategoryId = 1;
        private const int SecondCategoryId = 2;
        private const string ElectronicsTitle = "Electronics";
        private const string ElectronicsDescription = "Electronic devices";
        private const string ClothingTitle = "Clothing";
        private const string ClothingDescription = "Apparel items";
        private const string TestCategoryTitle = "Test Category";
        private const string TestCategoryDescription = "Test Description";
        private const string CategoryToDeleteTitle = "Category to Delete";
        private const string CategoryToDeleteDescription = "Description";
        private const int ExpectedSingleItem = 1;
        private const int ExpectedTwoItems = 2;
        private const int ExpectedZeroItems = 0;

        private ProductCategoryRepositoryMock _mockRepository;
        private IProductCategoryService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new ProductCategoryRepositoryMock();
            _service = new MarketMinds.Services.ProductCategoryService.ProductCategoryService(_mockRepository);
        }

        #region GetAllProductCategories Tests

        [Test]
        public void GetAllProductCategories_ReturnsCorrectNumberOfCategories()
        {
            // Arrange
            AddTestCategories();

            // Act
            var result = _service.GetAllProductCategories();

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedTwoItems));
        }

        [Test]
        public void GetAllProductCategories_ReturnsFirstCategoryWithCorrectTitle()
        {
            // Arrange
            AddTestCategories();

            // Act
            var result = _service.GetAllProductCategories();

            // Assert
            Assert.That(result[0].DisplayTitle, Is.EqualTo(ElectronicsTitle));
        }

        [Test]
        public void GetAllProductCategories_ReturnsSecondCategoryWithCorrectTitle()
        {
            // Arrange
            AddTestCategories();

            // Act
            var result = _service.GetAllProductCategories();

            // Assert
            Assert.That(result[1].DisplayTitle, Is.EqualTo(ClothingTitle));
        }

        #endregion

        #region CreateProductCategory Tests

        [Test]
        public void CreateProductCategory_ReturnsNewCategoryWithCorrectTitle()
        {
            // Act
            var result = _service.CreateProductCategory(TestCategoryTitle, TestCategoryDescription);

            // Assert
            Assert.That(result.DisplayTitle, Is.EqualTo(TestCategoryTitle));
        }

        [Test]
        public void CreateProductCategory_ReturnsNewCategoryWithCorrectDescription()
        {
            // Act
            var result = _service.CreateProductCategory(TestCategoryTitle, TestCategoryDescription);

            // Assert
            Assert.That(result.Description, Is.EqualTo(TestCategoryDescription));
        }

        [Test]
        public void CreateProductCategory_ReturnsNewCategoryWithCorrectId()
        {
            // Act
            var result = _service.CreateProductCategory(TestCategoryTitle, TestCategoryDescription);

            // Assert
            Assert.That(result.Id, Is.EqualTo(FirstCategoryId));
        }

        [Test]
        public void CreateProductCategory_AddsNewCategoryToRepository()
        {
            // Act
            _service.CreateProductCategory(TestCategoryTitle, TestCategoryDescription);

            // Assert
            Assert.That(_mockRepository.Categories.Count, Is.EqualTo(ExpectedSingleItem));
        }

        #endregion

        #region DeleteProductCategory Tests

        [Test]
        public void DeleteProductCategory_RemovesCategoryFromList()
        {
            // Arrange
            AddCategoryToDelete();
            Assert.That(_mockRepository.Categories.Count, Is.EqualTo(ExpectedSingleItem),
                "Precondition: Repository should have exactly one category before deletion");

            // Act
            _service.DeleteProductCategory(CategoryToDeleteTitle);

            // Assert
            Assert.That(_mockRepository.Categories.Count, Is.EqualTo(ExpectedZeroItems));
        }

        #endregion

        #region Helper Methods

        private void AddTestCategories()
        {
            _mockRepository.Categories.Add(new ProductCategory(FirstCategoryId, ElectronicsTitle, ElectronicsDescription));
            _mockRepository.Categories.Add(new ProductCategory(SecondCategoryId, ClothingTitle, ClothingDescription));
        }

        private void AddCategoryToDelete()
        {
            _mockRepository.Categories.Add(new ProductCategory(FirstCategoryId, CategoryToDeleteTitle, CategoryToDeleteDescription));
        }

        #endregion
    }
}