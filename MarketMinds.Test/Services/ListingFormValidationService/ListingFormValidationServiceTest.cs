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
        // Constants to replace magic strings and values
        private const int CategoryId = 1;
        private const string CategoryTitle = "Electronics";
        private const string CategoryDescription = "Electronic devices";
        private const int ConditionId = 1;
        private const string ConditionTitle = "New";
        private const string ConditionDescription = "Brand new item";
        private const string Tag1 = "tag1";
        private const string Tag2 = "tag2";
        private const string ValidTitle = "Test Product";
        private const string ValidDescription = "This is a test product description";
        private const string EmptyString = "";
        private const string WhitespaceTitle = "   ";
        private const string InvalidPriceText = "not a price";
        private const string NegativePriceText = "-50.00";
        private const string ValidPriceText = "99.99";
        private const string ValidDailyRateText = "25.50";
        private const string ValidStartingPriceText = "50.00";
        private const string TitleErrorMessage = "Title cannot be empty.";
        private const string CategoryErrorMessage = "Please select a category.";
        private const string DescriptionErrorMessage = "Description cannot be empty.";
        private const string TagsErrorMessage = "Please add at least one tag.";
        private const string ConditionErrorMessage = "Please select a condition.";
        private const string TitleField = "Title";
        private const string CategoryField = "Category";
        private const string DescriptionField = "Description";
        private const string TagsField = "Tags";
        private const string ConditionField = "Condition";
        private const float ValidPrice = 99.99f;
        private const float NegativePrice = -50.0f;
        private const float ZeroPrice = 0f;
        private const float ValidDailyRate = 25.5f;
        private const float ValidStartingPrice = 50.0f;
        private const int FutureDaysNormal = 30;
        private const int FutureDaysShort = 7;
        private const int PastDaysLong = -30;
        private const int PastDaysShort = -7;

        private ListingFormValidationService _validationService;
        private ProductCategory _validCategory;
        private ProductCondition _validCondition;
        private ObservableCollection<string> _validTags;

        [SetUp]
        public void Setup()
        {
            _validationService = new ListingFormValidationService();
            _validCategory = new ProductCategory(CategoryId, CategoryTitle, CategoryDescription);
            _validCondition = new ProductCondition(ConditionId, ConditionTitle, ConditionDescription);
            _validTags = new ObservableCollection<string> { Tag1, Tag2 };
        }

        #region ValidateCommonFields - Success Cases

        [Test]
        public void ValidateCommonFields_WithValidInputs_ReturnsTrue()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateCommonFields_WithValidInputs_SetsEmptyErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.Empty);
        }

        [Test]
        public void ValidateCommonFields_WithValidInputs_SetsEmptyErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.Empty);
        }

        #endregion

        #region ValidateCommonFields - Title Validation

        [Test]
        public void ValidateCommonFields_WithEmptyTitle_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                EmptyString, _validCategory, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTitle_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                EmptyString, _validCategory, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(TitleErrorMessage));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTitle_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                EmptyString, _validCategory, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(TitleField));
        }

        [Test]
        public void ValidateCommonFields_WithWhitespaceTitle_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                WhitespaceTitle, _validCategory, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithWhitespaceTitle_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                WhitespaceTitle, _validCategory, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(TitleErrorMessage));
        }

        [Test]
        public void ValidateCommonFields_WithWhitespaceTitle_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                WhitespaceTitle, _validCategory, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(TitleField));
        }

        #endregion

        #region ValidateCommonFields - Category Validation

        [Test]
        public void ValidateCommonFields_WithNullCategory_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                ValidTitle, null, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithNullCategory_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, null, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(CategoryErrorMessage));
        }

        [Test]
        public void ValidateCommonFields_WithNullCategory_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, null, ValidDescription, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(CategoryField));
        }

        #endregion

        #region ValidateCommonFields - Description Validation

        [Test]
        public void ValidateCommonFields_WithEmptyDescription_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, EmptyString, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithEmptyDescription_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, EmptyString, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(DescriptionErrorMessage));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyDescription_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, EmptyString, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(DescriptionField));
        }

        #endregion

        #region ValidateCommonFields - Tags Validation

        [Test]
        public void ValidateCommonFields_WithNullTags_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, null, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithNullTags_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, null, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(TagsErrorMessage));
        }

        [Test]
        public void ValidateCommonFields_WithNullTags_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, null, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(TagsField));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTags_ReturnsFalse()
        {
            // Arrange
            ObservableCollection<string> emptyTags = new ObservableCollection<string>();

            // Act
            bool result = _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, emptyTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTags_SetsCorrectErrorMessage()
        {
            // Arrange
            ObservableCollection<string> emptyTags = new ObservableCollection<string>();

            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, emptyTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(TagsErrorMessage));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTags_SetsCorrectErrorField()
        {
            // Arrange
            ObservableCollection<string> emptyTags = new ObservableCollection<string>();

            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, emptyTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(TagsField));
        }

        #endregion

        #region ValidateCommonFields - Condition Validation

        [Test]
        public void ValidateCommonFields_WithNullCondition_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, _validTags, null,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithNullCondition_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, _validTags, null,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(ConditionErrorMessage));
        }

        [Test]
        public void ValidateCommonFields_WithNullCondition_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                ValidTitle, _validCategory, ValidDescription, _validTags, null,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(ConditionField));
        }

        #endregion

        #region ValidateBuyProductFields

        [Test]
        public void ValidateBuyProductFields_WithValidPrice_ReturnsTrue()
        {
            // Act
            bool result = _validationService.ValidateBuyProductFields(ValidPriceText, out float price);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateBuyProductFields_WithValidPrice_SetsCorrectPrice()
        {
            // Act
            _validationService.ValidateBuyProductFields(ValidPriceText, out float price);

            // Assert
            Assert.That(price, Is.EqualTo(ValidPrice));
        }

        [Test]
        public void ValidateBuyProductFields_WithInvalidPrice_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateBuyProductFields(InvalidPriceText, out float price);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateBuyProductFields_WithInvalidPrice_SetsZeroPrice()
        {
            // Act
            _validationService.ValidateBuyProductFields(InvalidPriceText, out float price);

            // Assert
            Assert.That(price, Is.EqualTo(ZeroPrice));
        }

        [Test]
        public void ValidateBuyProductFields_WithNegativePrice_ReturnsTrue()
        {
            // This test identifies a potential issue in the validation logic
            // Act
            bool result = _validationService.ValidateBuyProductFields(NegativePriceText, out float price);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateBuyProductFields_WithNegativePrice_SetsNegativePrice()
        {
            // Act
            _validationService.ValidateBuyProductFields(NegativePriceText, out float price);

            // Assert
            Assert.That(price, Is.EqualTo(NegativePrice));
        }

        #endregion

        #region ValidateBorrowProductFields

        [Test]
        public void ValidateBorrowProductFields_WithValidInputs_ReturnsTrue()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(FutureDaysNormal);

            // Act
            bool result = _validationService.ValidateBorrowProductFields(ValidDailyRateText, timeLimit, out float dailyRate);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateBorrowProductFields_WithValidInputs_SetsCorrectDailyRate()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(FutureDaysNormal);

            // Act
            _validationService.ValidateBorrowProductFields(ValidDailyRateText, timeLimit, out float dailyRate);

            // Assert
            Assert.That(dailyRate, Is.EqualTo(ValidDailyRate));
        }

        [Test]
        public void ValidateBorrowProductFields_WithInvalidDailyRate_ReturnsFalse()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(FutureDaysNormal);

            // Act
            bool result = _validationService.ValidateBorrowProductFields(InvalidPriceText, timeLimit, out float dailyRate);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateBorrowProductFields_WithInvalidDailyRate_SetsZeroDailyRate()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(FutureDaysNormal);

            // Act
            _validationService.ValidateBorrowProductFields(InvalidPriceText, timeLimit, out float dailyRate);

            // Assert
            Assert.That(dailyRate, Is.EqualTo(ZeroPrice));
        }

        [Test]
        public void ValidateBorrowProductFields_WithPastTimeLimit_ReturnsFalse()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(PastDaysLong);

            // Act
            bool result = _validationService.ValidateBorrowProductFields(ValidDailyRateText, timeLimit, out float dailyRate);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateBorrowProductFields_WithPastTimeLimit_SetsCorrectDailyRate()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(PastDaysLong);

            // Act
            _validationService.ValidateBorrowProductFields(ValidDailyRateText, timeLimit, out float dailyRate);

            // Assert
            Assert.That(dailyRate, Is.EqualTo(ValidDailyRate));
        }

        [Test]
        public void ValidateBorrowProductFields_WithNullTimeLimit_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateBorrowProductFields(ValidDailyRateText, null, out float dailyRate);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateBorrowProductFields_WithNullTimeLimit_SetsCorrectDailyRate()
        {
            // Act
            _validationService.ValidateBorrowProductFields(ValidDailyRateText, null, out float dailyRate);

            // Assert
            Assert.That(dailyRate, Is.EqualTo(ValidDailyRate));
        }

        #endregion

        #region ValidateAuctionProductFields

        [Test]
        public void ValidateAuctionProductFields_WithValidInputs_ReturnsTrue()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(FutureDaysShort);

            // Act
            bool result = _validationService.ValidateAuctionProductFields(ValidStartingPriceText, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateAuctionProductFields_WithValidInputs_SetsCorrectStartingPrice()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(FutureDaysShort);

            // Act
            _validationService.ValidateAuctionProductFields(ValidStartingPriceText, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(startingPrice, Is.EqualTo(ValidStartingPrice));
        }

        [Test]
        public void ValidateAuctionProductFields_WithInvalidStartingPrice_ReturnsFalse()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(FutureDaysShort);

            // Act
            bool result = _validationService.ValidateAuctionProductFields(InvalidPriceText, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateAuctionProductFields_WithInvalidStartingPrice_SetsZeroStartingPrice()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(FutureDaysShort);

            // Act
            _validationService.ValidateAuctionProductFields(InvalidPriceText, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(startingPrice, Is.EqualTo(ZeroPrice));
        }

        [Test]
        public void ValidateAuctionProductFields_WithPastEndDate_ReturnsFalse()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(PastDaysShort);

            // Act
            bool result = _validationService.ValidateAuctionProductFields(ValidStartingPriceText, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateAuctionProductFields_WithPastEndDate_SetsCorrectStartingPrice()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(PastDaysShort);

            // Act
            _validationService.ValidateAuctionProductFields(ValidStartingPriceText, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(startingPrice, Is.EqualTo(ValidStartingPrice));
        }

        #endregion
    }
}

