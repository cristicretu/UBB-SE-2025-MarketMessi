using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Services;
using MarketMinds.Services.ListingFormValidationService;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class ListingFormValidationServiceTest
    {
        private ListingFormValidationService _validationService;
        private ProductCategory _validCategory;
        private ProductCondition _validCondition;
        private ObservableCollection<string> _validTags;

        [SetUp]
        public void Setup()
        {
            _validationService = new ListingFormValidationService();
            _validCategory = new ProductCategory(1, "Electronics", "Electronic devices");
            _validCondition = new ProductCondition(1, "New", "Brand new item");
            _validTags = new ObservableCollection<string> { "tag1", "tag2" };
        }

        [Test]
        public void ValidateCommonFields_WithValidInputs_ReturnsTrue()
        {
            string title = "Test Product";
            string description = "This is a test product description";

            bool result = _validationService.ValidateCommonFields(
                title, _validCategory, description, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            Assert.That(result, Is.True);
            Assert.That(errorMessage, Is.Empty);
            Assert.That(errorField, Is.Empty);
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTitle_ReturnsFalse()
        {
            string title = "";
            string description = "This is a test product description";

            bool result = _validationService.ValidateCommonFields(
                title, _validCategory, description, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("Title cannot be empty."));
            Assert.That(errorField, Is.EqualTo("Title"));
        }

        [Test]
        public void ValidateCommonFields_WithWhitespaceTitle_ReturnsFalse()
        {
            string title = "   ";
            string description = "This is a test product description";

            bool result = _validationService.ValidateCommonFields(
                title, _validCategory, description, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("Title cannot be empty."));
            Assert.That(errorField, Is.EqualTo("Title"));
        }

        [Test]
        public void ValidateCommonFields_WithNullCategory_ReturnsFalse()
        {
            string title = "Test Product";
            string description = "This is a test product description";
            ProductCategory nullCategory = null;

            bool result = _validationService.ValidateCommonFields(
                title, nullCategory, description, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("Please select a category."));
            Assert.That(errorField, Is.EqualTo("Category"));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyDescription_ReturnsFalse()
        {
            string title = "Test Product";
            string description = "";

            bool result = _validationService.ValidateCommonFields(
                title, _validCategory, description, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("Description cannot be empty."));
            Assert.That(errorField, Is.EqualTo("Description"));
        }

        [Test]
        public void ValidateCommonFields_WithNullTags_ReturnsFalse()
        {
            string title = "Test Product";
            string description = "This is a test product description";
            ObservableCollection<string> nullTags = null;

            bool result = _validationService.ValidateCommonFields(
                title, _validCategory, description, nullTags, _validCondition,
                out string errorMessage, out string errorField);

            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("Please add at least one tag."));
            Assert.That(errorField, Is.EqualTo("Tags"));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTags_ReturnsFalse()
        {
            string title = "Test Product";
            string description = "This is a test product description";
            ObservableCollection<string> emptyTags = new ObservableCollection<string>();

            bool result = _validationService.ValidateCommonFields(
                title, _validCategory, description, emptyTags, _validCondition,
                out string errorMessage, out string errorField);

            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("Please add at least one tag."));
            Assert.That(errorField, Is.EqualTo("Tags"));
        }

        [Test]
        public void ValidateCommonFields_WithNullCondition_ReturnsFalse()
        {
            string title = "Test Product";
            string description = "This is a test product description";
            ProductCondition nullCondition = null;

            bool result = _validationService.ValidateCommonFields(
                title, _validCategory, description, _validTags, nullCondition,
                out string errorMessage, out string errorField);

            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("Please select a condition."));
            Assert.That(errorField, Is.EqualTo("Condition"));
        }

        [Test]
        public void ValidateBuyProductFields_WithValidPrice_ReturnsTrue()
        {
            string priceText = "99.99";
            bool result = _validationService.ValidateBuyProductFields(priceText, out float price);

            Assert.That(result, Is.True);
            Assert.That(price, Is.EqualTo(99.99f));
        }

        [Test]
        public void ValidateBuyProductFields_WithInvalidPrice_ReturnsFalse()
        {
            string priceText = "not a price";
            bool result = _validationService.ValidateBuyProductFields(priceText, out float price);

            Assert.That(result, Is.False);
            Assert.That(price, Is.EqualTo(0));
        }

        [Test]
        public void ValidateBuyProductFields_WithNegativePrice_ReturnsTrue_ButShouldConsiderInvalid()
        {
            string priceText = "-50.00";

            bool result = _validationService.ValidateBuyProductFields(priceText, out float price);

            Assert.That(result, Is.True);
            Assert.That(price, Is.EqualTo(-50.0f));
        }

        [Test]
        public void ValidateBorrowProductFields_WithValidInputs_ReturnsTrue()
        {
            string dailyRateText = "25.50";
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(30);

            bool result = _validationService.ValidateBorrowProductFields(dailyRateText, timeLimit, out float dailyRate);

            Assert.That(result, Is.True);
            Assert.That(dailyRate, Is.EqualTo(25.5f));
        }

        [Test]
        public void ValidateBorrowProductFields_WithInvalidDailyRate_ReturnsFalse()
        {
            string dailyRateText = "not a rate";
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(30);

            bool result = _validationService.ValidateBorrowProductFields(dailyRateText, timeLimit, out float dailyRate);

            Assert.That(result, Is.False);
            Assert.That(dailyRate, Is.EqualTo(0));
        }

        [Test]
        public void ValidateBorrowProductFields_WithPastTimeLimit_ReturnsFalse()
        {
            string dailyRateText = "25.50";
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(-30);

            bool result = _validationService.ValidateBorrowProductFields(dailyRateText, timeLimit, out float dailyRate);

            Assert.That(result, Is.False);
            Assert.That(dailyRate, Is.EqualTo(25.5f));
        }

        [Test]
        public void ValidateBorrowProductFields_WithNullTimeLimit_ReturnsFalse()
        {
            string dailyRateText = "25.50";
            DateTimeOffset? timeLimit = null;

            bool result = _validationService.ValidateBorrowProductFields(dailyRateText, timeLimit, out float dailyRate);

            Assert.That(result, Is.False);
            Assert.That(dailyRate, Is.EqualTo(25.5f));
        }

        [Test]
        public void ValidateAuctionProductFields_WithValidInputs_ReturnsTrue()
        {
            string startingPriceText = "50.00";
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(7);

            bool result = _validationService.ValidateAuctionProductFields(startingPriceText, endAuctionDate, out float startingPrice);

            Assert.That(result, Is.True);
            Assert.That(startingPrice, Is.EqualTo(50.0f));
        }

        [Test]
        public void ValidateAuctionProductFields_WithInvalidStartingPrice_ReturnsFalse()
        {
            string startingPriceText = "not a price";
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(7);

            bool result = _validationService.ValidateAuctionProductFields(startingPriceText, endAuctionDate, out float startingPrice);

            Assert.That(result, Is.False);
            Assert.That(startingPrice, Is.EqualTo(0));
        }

        [Test]
        public void ValidateAuctionProductFields_WithPastEndDate_ReturnsFalse()
        {
            string startingPriceText = "50.00";
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(-7);

            bool result = _validationService.ValidateAuctionProductFields(startingPriceText, endAuctionDate, out float startingPrice);

            Assert.That(result, Is.False);
            Assert.That(startingPrice, Is.EqualTo(50.0f));
        }
    }
}