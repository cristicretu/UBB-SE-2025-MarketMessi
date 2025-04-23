using DomainLayer.Domain;
using MarketMinds.Repositories.ProductTagRepository;
using MarketMinds.Services.ProductTagService;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MarketMinds.Test.Services.ProductTagService
{
    [TestFixture]
    public class ProductTagServiceTests
    {
        // Constants to replace magic strings and numbers
        private const int TagId1 = 1;
        private const int TagId2 = 2;
        private const int TagId3 = 3;
        private const string ElectronicsTagTitle = "Electronics";
        private const string ClothingTagTitle = "Clothing";
        private const string BooksTagTitle = "Books";
        private const string GamingTagTitle = "Gaming";
        private const string FurnitureTagTitle = "Furniture";
        private const int ExpectedEmptyCount = 0;
        private const int ExpectedSingleTagCount = 1;
        private const int ExpectedThreeTagsCount = 3;

        private ProductTagRepositoryMock _repositoryMock;
        private IProductTagService _service;

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new ProductTagRepositoryMock();
            _service = new MarketMinds.Services.ProductTagService.ProductTagService(_repositoryMock);
        }

        #region GetAllProductTags Tests

        [Test]
        public void GetAllProductTags_ReturnsNonNullResult()
        {
            // Arrange
            AddThreeSampleTags();

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectNumberOfTags()
        {
            // Arrange
            AddThreeSampleTags();

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedThreeTagsCount));
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectFirstTagTitle()
        {
            // Arrange
            AddThreeSampleTags();

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result[0].DisplayTitle, Is.EqualTo(ElectronicsTagTitle));
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectSecondTagTitle()
        {
            // Arrange
            AddThreeSampleTags();

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result[1].DisplayTitle, Is.EqualTo(ClothingTagTitle));
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectThirdTagTitle()
        {
            // Arrange
            AddThreeSampleTags();

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result[2].DisplayTitle, Is.EqualTo(BooksTagTitle));
        }

        [Test]
        public void GetAllProductTags_WithEmptyRepository_ReturnsNonNullResult()
        {
            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetAllProductTags_WithEmptyRepository_ReturnsEmptyList()
        {
            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result.Count, Is.EqualTo(ExpectedEmptyCount));
        }

        #endregion

        #region CreateProductTag Tests

        [Test]
        public void CreateProductTag_ReturnsNonNullResult()
        {
            // Act
            var result = _service.CreateProductTag(GamingTagTitle);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CreateProductTag_ReturnsTagWithCorrectTitle()
        {
            // Act
            var result = _service.CreateProductTag(GamingTagTitle);

            // Assert
            Assert.That(result.DisplayTitle, Is.EqualTo(GamingTagTitle));
        }

        [Test]
        public void CreateProductTag_ReturnsTagWithCorrectId()
        {
            // Act
            var result = _service.CreateProductTag(GamingTagTitle);

            // Assert
            Assert.That(result.Id, Is.EqualTo(TagId1));
        }

        [Test]
        public void CreateProductTag_AddsTagToRepository()
        {
            // Act
            _service.CreateProductTag(GamingTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(ExpectedSingleTagCount));
        }

        [Test]
        public void CreateProductTag_AddsTagWithCorrectTitleToRepository()
        {
            // Act
            _service.CreateProductTag(GamingTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.First().DisplayTitle, Is.EqualTo(GamingTagTitle));
        }

        #endregion

        #region CreateProductTag Multiple Tags Tests

        [Test]
        public void CreateProductTag_MultipleTags_FirstTagHasCorrectId()
        {
            // Act
            var tag1 = _service.CreateProductTag(ElectronicsTagTitle);

            // Assert
            Assert.That(tag1.Id, Is.EqualTo(TagId1));
        }

        [Test]
        public void CreateProductTag_MultipleTags_SecondTagHasCorrectId()
        {
            // Act
            _service.CreateProductTag(ElectronicsTagTitle);
            var tag2 = _service.CreateProductTag(ClothingTagTitle);

            // Assert
            Assert.That(tag2.Id, Is.EqualTo(TagId2));
        }

        [Test]
        public void CreateProductTag_MultipleTags_ThirdTagHasCorrectId()
        {
            // Act
            _service.CreateProductTag(ElectronicsTagTitle);
            _service.CreateProductTag(ClothingTagTitle);
            var tag3 = _service.CreateProductTag(BooksTagTitle);

            // Assert
            Assert.That(tag3.Id, Is.EqualTo(TagId3));
        }

        [Test]
        public void CreateProductTag_MultipleTags_AddsAllTagsToRepository()
        {
            // Act
            _service.CreateProductTag(ElectronicsTagTitle);
            _service.CreateProductTag(ClothingTagTitle);
            _service.CreateProductTag(BooksTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(ExpectedThreeTagsCount));
        }

        [Test]
        public void CreateProductTag_MultipleTags_EnsuresUniqueIds()
        {
            // Act
            var tag1 = _service.CreateProductTag(ElectronicsTagTitle);
            var tag2 = _service.CreateProductTag(ClothingTagTitle);

            // Assert
            Assert.That(tag1.Id, Is.Not.EqualTo(tag2.Id));
        }

        #endregion

        #region DeleteProductTag Tests

        [Test]
        public void DeleteProductTag_ExistingTag_RemovesTagFromRepository()
        {
            // Arrange
            AddSingleTag(TagId1, ElectronicsTagTitle);
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(ExpectedSingleTagCount),
                "Precondition: Repository should have one tag before deletion");

            // Act
            _service.DeleteProductTag(ElectronicsTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(ExpectedEmptyCount));
        }

        [Test]
        public void DeleteProductTag_ExistingTag_RemovesCorrectTag()
        {
            // Arrange
            AddSingleTag(TagId1, ElectronicsTagTitle);

            // Act
            _service.DeleteProductTag(ElectronicsTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.Any(t => t.DisplayTitle == ElectronicsTagTitle), Is.False);
        }

        [Test]
        public void DeleteProductTag_NonExistentTag_DoesNotChangeRepositoryCount()
        {
            // Arrange
            AddSingleTag(TagId1, ElectronicsTagTitle);
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(ExpectedSingleTagCount),
                "Precondition: Repository should have one tag before operation");

            // Act
            _service.DeleteProductTag(FurnitureTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(ExpectedSingleTagCount));
        }

        [Test]
        public void DeleteProductTag_NonExistentTag_KeepsExistingTagUnchanged()
        {
            // Arrange
            AddSingleTag(TagId1, ElectronicsTagTitle);

            // Act
            _service.DeleteProductTag(FurnitureTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.First().DisplayTitle, Is.EqualTo(ElectronicsTagTitle));
        }

        [Test]
        public void DeleteProductTag_WithMultipleTagsWithSameTitle_RemovesAllMatchingTags()
        {
            // Arrange
            AddTagsWithDuplicateTitle();
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(ExpectedThreeTagsCount),
                "Precondition: Repository should have three tags before deletion");

            // Act
            _service.DeleteProductTag(ElectronicsTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(ExpectedSingleTagCount));
        }

        [Test]
        public void DeleteProductTag_WithMultipleTagsWithSameTitle_KeepsNonMatchingTags()
        {
            // Arrange
            AddTagsWithDuplicateTitle();

            // Act
            _service.DeleteProductTag(ElectronicsTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.First().DisplayTitle, Is.EqualTo(BooksTagTitle));
        }

        [Test]
        public void DeleteProductTag_WithMultipleTagsWithSameTitle_RemovesAllInstancesOfMatchingTitle()
        {
            // Arrange
            AddTagsWithDuplicateTitle();

            // Act
            _service.DeleteProductTag(ElectronicsTagTitle);

            // Assert
            Assert.That(_repositoryMock.Tags.Any(t => t.DisplayTitle == ElectronicsTagTitle), Is.False);
        }

        #endregion

        #region Helper Methods

        private void AddThreeSampleTags()
        {
            _repositoryMock.Tags.AddRange(new List<ProductTag>
            {
                new ProductTag(TagId1, ElectronicsTagTitle),
                new ProductTag(TagId2, ClothingTagTitle),
                new ProductTag(TagId3, BooksTagTitle)
            });
        }

        private void AddSingleTag(int id, string title)
        {
            _repositoryMock.Tags.Add(new ProductTag(id, title));
        }

        private void AddTagsWithDuplicateTitle()
        {
            _repositoryMock.Tags.AddRange(new List<ProductTag>
            {
                new ProductTag(TagId1, ElectronicsTagTitle),
                new ProductTag(TagId2, BooksTagTitle),
                new ProductTag(TagId3, ElectronicsTagTitle)
            });
        }

        #endregion
    }
}