using MarketMinds.Shared.Models;
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
        private const int TAG_ID_1 = 1;
        private const int TAG_ID_2 = 2;
        private const int TAG_ID_3 = 3;
        private const string ELECTRONICS_TAG_TITLE = "Electronics";
        private const string CLOTHING_TAG_TITLE = "Clothing";
        private const string BOOKS_TAG_TITLE = "Books";
        private const string GAMING_TAG_TITLE = "Gaming";
        private const string FURNITURE_TAG_TITLE = "Furniture";
        private const int EXPECTED_EMPTY_COUNT = 0;
        private const int EXPECTED_SINGLE_TAG_COUNT = 1;
        private const int EXPECTED_THREE_TAGS_COUNT = 3;

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
            Assert.That(result.Count, Is.EqualTo(EXPECTED_THREE_TAGS_COUNT));
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectFirstTagTitle()
        {
            // Arrange
            AddThreeSampleTags();

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result[0].DisplayTitle, Is.EqualTo(ELECTRONICS_TAG_TITLE));
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectSecondTagTitle()
        {
            // Arrange
            AddThreeSampleTags();

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result[1].DisplayTitle, Is.EqualTo(CLOTHING_TAG_TITLE));
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectThirdTagTitle()
        {
            // Arrange
            AddThreeSampleTags();

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result[2].DisplayTitle, Is.EqualTo(BOOKS_TAG_TITLE));
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
            Assert.That(result.Count, Is.EqualTo(EXPECTED_EMPTY_COUNT));
        }

        #endregion

        #region CreateProductTag Tests

        [Test]
        public void CreateProductTag_ReturnsNonNullResult()
        {
            // Act
            var result = _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CreateProductTag_ReturnsTagWithCorrectTitle()
        {
            // Act
            var result = _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            Assert.That(result.DisplayTitle, Is.EqualTo(GAMING_TAG_TITLE));
        }

        [Test]
        public void CreateProductTag_ReturnsTagWithCorrectId()
        {
            // Act
            var result = _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            Assert.That(result.Id, Is.EqualTo(TAG_ID_1));
        }

        [Test]
        public void CreateProductTag_AddsTagToRepository()
        {
            // Act
            _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(EXPECTED_SINGLE_TAG_COUNT));
        }

        [Test]
        public void CreateProductTag_AddsTagWithCorrectTitleToRepository()
        {
            // Act
            _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.First().DisplayTitle, Is.EqualTo(GAMING_TAG_TITLE));
        }

        #endregion

        #region CreateProductTag Multiple Tags Tests

        [Test]
        public void CreateProductTag_MultipleTags_FirstTagHasCorrectId()
        {
            // Act
            var tag1 = _service.CreateProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            Assert.That(tag1.Id, Is.EqualTo(TAG_ID_1));
        }

        [Test]
        public void CreateProductTag_MultipleTags_SecondTagHasCorrectId()
        {
            // Act
            _service.CreateProductTag(ELECTRONICS_TAG_TITLE);
            var tag2 = _service.CreateProductTag(CLOTHING_TAG_TITLE);

            // Assert
            Assert.That(tag2.Id, Is.EqualTo(TAG_ID_2));
        }

        [Test]
        public void CreateProductTag_MultipleTags_ThirdTagHasCorrectId()
        {
            // Act
            _service.CreateProductTag(ELECTRONICS_TAG_TITLE);
            _service.CreateProductTag(CLOTHING_TAG_TITLE);
            var tag3 = _service.CreateProductTag(BOOKS_TAG_TITLE);

            // Assert
            Assert.That(tag3.Id, Is.EqualTo(TAG_ID_3));
        }

        [Test]
        public void CreateProductTag_MultipleTags_AddsAllTagsToRepository()
        {
            // Act
            _service.CreateProductTag(ELECTRONICS_TAG_TITLE);
            _service.CreateProductTag(CLOTHING_TAG_TITLE);
            _service.CreateProductTag(BOOKS_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(EXPECTED_THREE_TAGS_COUNT));
        }

        [Test]
        public void CreateProductTag_MultipleTags_EnsuresUniqueIds()
        {
            // Act
            var tag1 = _service.CreateProductTag(ELECTRONICS_TAG_TITLE);
            var tag2 = _service.CreateProductTag(CLOTHING_TAG_TITLE);

            // Assert
            Assert.That(tag1.Id, Is.Not.EqualTo(tag2.Id));
        }

        #endregion

        #region DeleteProductTag Tests

        [Test]
        public void DeleteProductTag_ExistingTag_RemovesTagFromRepository()
        {
            // Arrange
            AddSingleTag(TAG_ID_1, ELECTRONICS_TAG_TITLE);
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(EXPECTED_SINGLE_TAG_COUNT),
                "Precondition: Repository should have one tag before deletion");

            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(EXPECTED_EMPTY_COUNT));
        }

        [Test]
        public void DeleteProductTag_ExistingTag_RemovesCorrectTag()
        {
            // Arrange
            AddSingleTag(TAG_ID_1, ELECTRONICS_TAG_TITLE);

            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.Any(t => t.DisplayTitle == ELECTRONICS_TAG_TITLE), Is.False);
        }

        [Test]
        public void DeleteProductTag_NonExistentTag_DoesNotChangeRepositoryCount()
        {
            // Arrange
            AddSingleTag(TAG_ID_1, ELECTRONICS_TAG_TITLE);
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(EXPECTED_SINGLE_TAG_COUNT),
                "Precondition: Repository should have one tag before operation");

            // Act
            _service.DeleteProductTag(FURNITURE_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(EXPECTED_SINGLE_TAG_COUNT));
        }

        [Test]
        public void DeleteProductTag_NonExistentTag_KeepsExistingTagUnchanged()
        {
            // Arrange
            AddSingleTag(TAG_ID_1, ELECTRONICS_TAG_TITLE);

            // Act
            _service.DeleteProductTag(FURNITURE_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.First().DisplayTitle, Is.EqualTo(ELECTRONICS_TAG_TITLE));
        }

        [Test]
        public void DeleteProductTag_WithMultipleTagsWithSameTitle_RemovesAllMatchingTags()
        {
            // Arrange
            AddTagsWithDuplicateTitle();
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(EXPECTED_THREE_TAGS_COUNT),
                "Precondition: Repository should have three tags before deletion");

            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.Count, Is.EqualTo(EXPECTED_SINGLE_TAG_COUNT));
        }

        [Test]
        public void DeleteProductTag_WithMultipleTagsWithSameTitle_KeepsNonMatchingTags()
        {
            // Arrange
            AddTagsWithDuplicateTitle();

            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.First().DisplayTitle, Is.EqualTo(BOOKS_TAG_TITLE));
        }

        [Test]
        public void DeleteProductTag_WithMultipleTagsWithSameTitle_RemovesAllInstancesOfMatchingTitle()
        {
            // Arrange
            AddTagsWithDuplicateTitle();

            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            Assert.That(_repositoryMock.Tags.Any(t => t.DisplayTitle == ELECTRONICS_TAG_TITLE), Is.False);
        }

        #endregion

        #region Helper Methods

        private void AddThreeSampleTags()
        {
            _repositoryMock.Tags.AddRange(new List<ProductTag>
            {
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, CLOTHING_TAG_TITLE),
                new ProductTag(TAG_ID_3, BOOKS_TAG_TITLE)
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
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, BOOKS_TAG_TITLE),
                new ProductTag(TAG_ID_3, ELECTRONICS_TAG_TITLE)
            });
        }

        #endregion
    }
}