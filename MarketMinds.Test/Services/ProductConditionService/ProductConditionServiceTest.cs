using MarketMinds.Shared.Models;
using MarketMinds.Services.ProductConditionService;
using NUnit.Framework;

namespace MarketMinds.Test.Services.ProductConditionService
{
    [TestFixture]
    internal class ProductConditionServiceTest
    {
        // Constants to replace magic strings and numbers
        private const int FIRST_CONDITION_ID = 1;
        private const int SECOND_CONDITION_ID = 2;
        private const string NEW_CONDITION_TITLE = "New";
        private const string NEW_CONDITION_DESCRIPTION = "Never used";
        private const string USED_CONDITION_TITLE = "Used";
        private const string USED_CONDITION_DESCRIPTION = "Shows use marks";
        private const string CONDITION_TO_DELETE_TITLE = "Condition to Delete";
        private const string CONDITION_TO_DELETE_DESCRIPTION = "Description";
        private const int EXPECTED_SINGLE_ITEM = 1;
        private const int EXPECTED_TWO_ITEMS = 2;
        private const int EXPECTED_ZERO_ITEMS = 0;

        private ProductConditionRepositoryMock _mockRepository;
        private IProductConditionService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new ConditionRepositoryMock();
            _service = new MarketMinds.Services.ProductConditionService.ProductConditionService(_mockRepository);
        }

        #region GetAllProductConditions Tests

        [Test]
        public void GetAllProductConditions_ReturnsCorrectNumberOfConditions()
        {
            // Arrange
            AddTestConditions();

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result.Count, Is.EqualTo(EXPECTED_TWO_ITEMS));
        }

        [Test]
        public void GetAllProductConditions_FirstConditionHasCorrectTitle()
        {
            // Arrange
            AddTestConditions();

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[0].DisplayTitle, Is.EqualTo(NEW_CONDITION_TITLE));
        }

        [Test]
        public void GetAllProductConditions_SecondConditionHasCorrectTitle()
        {
            // Arrange
            AddTestConditions();

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[1].DisplayTitle, Is.EqualTo(USED_CONDITION_TITLE));
        }

        [Test]
        public void GetAllProductConditions_FirstConditionHasCorrectDescription()
        {
            // Arrange
            AddTestConditions();

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[0].Description, Is.EqualTo(NEW_CONDITION_DESCRIPTION));
        }

        [Test]
        public void GetAllProductConditions_SecondConditionHasCorrectDescription()
        {
            // Arrange
            AddTestConditions();

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[1].Description, Is.EqualTo(USED_CONDITION_DESCRIPTION));
        }

        #endregion

        #region CreateProductCondition Tests

        [Test]
        public void CreateProductCondition_ReturnsConditionWithCorrectTitle()
        {
            // Act
            var result = _service.CreateProductCondition(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION);

            // Assert
            Assert.That(result.DisplayTitle, Is.EqualTo(NEW_CONDITION_TITLE));
        }

        [Test]
        public void CreateProductCondition_ReturnsConditionWithCorrectDescription()
        {
            // Act
            var result = _service.CreateProductCondition(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION);

            // Assert
            Assert.That(result.Description, Is.EqualTo(NEW_CONDITION_DESCRIPTION));
        }

        [Test]
        public void CreateProductCondition_ReturnsConditionWithCorrectId()
        {
            // Act
            var result = _service.CreateProductCondition(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION);

            // Assert
            Assert.That(result.Id, Is.EqualTo(FIRST_CONDITION_ID));
        }

        [Test]
        public void CreateProductCondition_AddsConditionToRepository()
        {
            // Act
            _service.CreateProductCondition(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION);

            // Assert
            Assert.That(_mockRepository.Conditions.Count, Is.EqualTo(EXPECTED_SINGLE_ITEM));
        }

        #endregion

        #region DeleteProductCondition Tests

        [Test]
        public void DeleteProductCondition_RemovesConditionFromRepository()
        {
            // Arrange
            AddConditionToDelete();
            Assert.That(_mockRepository.Conditions.Count, Is.EqualTo(EXPECTED_SINGLE_ITEM),
                "Precondition: Repository should have exactly one condition before deletion");

            // Act
            _service.DeleteProductCondition(CONDITION_TO_DELETE_TITLE);

            // Assert
            Assert.That(_mockRepository.Conditions.Count, Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }

        #endregion

        #region Helper Methods

        private void AddTestConditions()
        {
            _mockRepository.Conditions.Add(new Condition(FIRST_CONDITION_ID, NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION));
            _mockRepository.Conditions.Add(new Condition(SECOND_CONDITION_ID, USED_CONDITION_TITLE, USED_CONDITION_DESCRIPTION));
        }

        private void AddConditionToDelete()
        {
            _mockRepository.Conditions.Add(new Condition(FIRST_CONDITION_ID, CONDITION_TO_DELETE_TITLE, CONDITION_TO_DELETE_DESCRIPTION));
        }

        #endregion
    }
}