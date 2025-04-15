using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DomainLayer.Domain;
using MarketMinds.Helpers;
using NUnit.Framework;
using ViewModelLayer.ViewModel;

namespace MarketMinds.Test.Helpers.TagManagementViewModelHelperTest
{
    [TestFixture]
    public class TagManagementViewModelHelperTest
    {
        private TestProductTagViewModel productTagViewModel;
        private TagManagementViewModelHelper helper;
        private ObservableCollection<string> tags;

        [SetUp]
        public void Setup()
        {
            // Use our test-specific implementation with proper constructor
            productTagViewModel = TagManagementViewModelHelperMocks.CreateTestProductTagViewModel();
            helper = new TagManagementViewModelHelper(productTagViewModel);
            tags = new ObservableCollection<string>();
        }

        [Test]
        public void AddTagToCollection_WithValidTag_ShouldAddTag()
        {
            // Arrange
            string tagToAdd = "testTag";

            // Act
            bool result = helper.AddTagToCollection(tagToAdd, tags);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(tags.Count, Is.EqualTo(1));
            Assert.That(tags[0], Is.EqualTo(tagToAdd));
        }

        [Test]
        public void AddTagToCollection_WithEmptyTag_ShouldNotAddTag()
        {
            // Arrange
            string emptyTag = string.Empty;

            // Act
            bool result = helper.AddTagToCollection(emptyTag, tags);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(tags.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddTagToCollection_WithDuplicateTag_ShouldNotAddTag()
        {
            // Arrange
            string tag = "testTag";
            tags.Add(tag);

            // Act
            bool result = helper.AddTagToCollection(tag, tags);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(tags.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddTagToCollection_WithTagWithWhitespace_ShouldTrimAndAddTag()
        {
            // Arrange
            string tagWithWhitespace = "  testTag  ";
            string expectedTag = "testTag";

            // Act
            bool result = helper.AddTagToCollection(tagWithWhitespace, tags);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(tags.Count, Is.EqualTo(1));
            Assert.That(tags[0], Is.EqualTo(expectedTag));
        }

        [Test]
        public void RemoveTagFromCollection_WithExistingTag_ShouldRemoveTag()
        {
            // Arrange
            string tag = "testTag";
            tags.Add(tag);

            // Act
            bool result = helper.RemoveTagFromCollection(tag, tags);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(tags.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveTagFromCollection_WithNonExistingTag_ShouldReturnFalse()
        {
            // Arrange
            string existingTag = "existingTag";
            string nonExistingTag = "nonExistingTag";
            tags.Add(existingTag);

            // Act
            bool result = helper.RemoveTagFromCollection(nonExistingTag, tags);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(tags.Count, Is.EqualTo(1));
            Assert.That(tags[0], Is.EqualTo(existingTag));
        }

        [Test]
        public void RemoveTagFromCollection_WithNullTag_ShouldReturnFalse()
        {
            // Arrange
            string existingTag = "existingTag";
            tags.Add(existingTag);

            // Act
            bool result = helper.RemoveTagFromCollection(null, tags);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(tags.Count, Is.EqualTo(1));
        }

        [Test]
        public void EnsureTagExists_WithExistingTag_ShouldReturnExistingTag()
        {
            // Arrange
            string tagName = "existingTag";
            var existingTag = new ProductTag(1, tagName);
            
            // Set up with existing tag
            productTagViewModel = TagManagementViewModelHelperMocks.SetupTagViewModel(
                new List<ProductTag> { existingTag });
            helper = new TagManagementViewModelHelper(productTagViewModel);

            // Act
            var result = helper.EnsureTagExists(tagName);

            // Assert
            Assert.That(result, Is.EqualTo(existingTag));
            // No new tags should be created
            Assert.That(productTagViewModel.GetAllProductTags().Count, Is.EqualTo(1));
        }

        [Test]
        public void EnsureTagExists_WithNonExistingTag_ShouldCreateAndReturnNewTag()
        {
            // Arrange
            string tagName = "newTag";
            
            // Set up empty test view model
            productTagViewModel = TagManagementViewModelHelperMocks.SetupTagViewModel();
            helper = new TagManagementViewModelHelper(productTagViewModel);

            // Act
            var result = helper.EnsureTagExists(tagName);

            // Assert
            Assert.That(result.DisplayTitle, Is.EqualTo(tagName));
            Assert.That(productTagViewModel.GetAllProductTags().Count, Is.EqualTo(1));
            Assert.That(productTagViewModel.GetAllProductTags()[0].DisplayTitle, Is.EqualTo(tagName));
        }

        [Test]
        public void ConvertStringTagsToProductTags_ShouldConvertAllTags()
        {
            // Arrange
            string[] stringTags = { "tag1", "tag2", "tag3" };
            
            // Set up empty test view model
            productTagViewModel = TagManagementViewModelHelperMocks.SetupTagViewModel();
            helper = new TagManagementViewModelHelper(productTagViewModel);

            // Act
            var result = helper.ConvertStringTagsToProductTags(stringTags);

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].DisplayTitle, Is.EqualTo("tag1"));
            Assert.That(result[1].DisplayTitle, Is.EqualTo("tag2"));
            Assert.That(result[2].DisplayTitle, Is.EqualTo("tag3"));
            
            // Verify that we created each tag
            Assert.That(productTagViewModel.GetAllProductTags().Count, Is.EqualTo(3));
            Assert.That(productTagViewModel.GetAllProductTags().Any(t => t.DisplayTitle == "tag1"), Is.True);
            Assert.That(productTagViewModel.GetAllProductTags().Any(t => t.DisplayTitle == "tag2"), Is.True);
            Assert.That(productTagViewModel.GetAllProductTags().Any(t => t.DisplayTitle == "tag3"), Is.True);
        }
    }
}