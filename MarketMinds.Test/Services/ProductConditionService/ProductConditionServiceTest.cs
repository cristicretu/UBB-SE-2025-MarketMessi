using DomainLayer.Domain;
using MarketMinds.Services.ProductConditionService;
using NUnit.Framework;

namespace MarketMinds.Test.Services.ProductConditionService
{
    [TestFixture]
    internal class ProductConditionServiceTest
    {
        // Constants to replace magic strings and numbers
        private const int FirstConditionId = 1;
        private const int SecondConditionId = 2;
        private const string NewConditionTitle = "New";
        private const string NewConditionDescription = "Never used";
        private const string UsedConditionTitle = "Used";
        private const string UsedConditionDescription = "Shows use marks";
        private const string ConditionToDeleteTitle = "Condition to Delete";
        private const string ConditionToDeleteDescription = "Description";
        private const int ExpectedSingleItem = 1;
        private const int ExpectedTwoItems = 2;
        private const int ExpectedZeroItems = 0;

        private ProductConditionRepositoryMock _mockRepository;
        private IProductConditionService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new ProductConditionRepositoryMock();
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
            Assert.That(result.Count, Is.EqualTo(ExpectedTwoItems));
        }

        [Test]
        public void GetAllProductConditions_FirstConditionHasCorrectTitle()
        {
            // Arrange
            AddTestConditions();

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[0].DisplayTitle, Is.EqualTo(NewConditionTitle));
        }

        [Test]
        public void GetAllProductConditions_SecondConditionHasCorrectTitle()
        {
            // Arrange
            AddTestConditions();

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[1].DisplayTitle, Is.EqualTo(UsedConditionTitle));
        }

        [Test]
        public void GetAllProductConditions_FirstConditionHasCorrectDescription()
        {
            // Arrange
            AddTestConditions();

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[0].Description, Is.EqualTo(NewConditionDescription));
        }

        [Test]
        public void GetAllProductConditions_SecondConditionHasCorrectDescription()
        {
            // Arrange
            AddTestConditions();

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[1].Description, Is.EqualTo(UsedConditionDescription));
        }

        #endregion

        #region CreateProductCondition Tests

        [Test]
        public void CreateProductCondition_ReturnsConditionWithCorrectTitle()
        {
            // Act
            var result = _service.CreateProductCondition(NewConditionTitle, NewConditionDescription);

            // Assert
            Assert.That(result.DisplayTitle, Is.EqualTo(NewConditionTitle));
        }

        [Test]
        public void CreateProductCondition_ReturnsConditionWithCorrectDescription()
        {
            // Act
            var result = _service.CreateProductCondition(NewConditionTitle, NewConditionDescription);

            // Assert
            Assert.That(result.Description, Is.EqualTo(NewConditionDescription));
        }

        [Test]
        public void CreateProductCondition_ReturnsConditionWithCorrectId()
        {
            // Act
            var result = _service.CreateProductCondition(NewConditionTitle, NewConditionDescription);

            // Assert
            Assert.That(result.Id, Is.EqualTo(FirstConditionId));
        }

        [Test]
        public void CreateProductCondition_AddsConditionToRepository()
        {
            // Act
            _service.CreateProductCondition(NewConditionTitle, NewConditionDescription);

            // Assert
            Assert.That(_mockRepository.Conditions.Count, Is.EqualTo(ExpectedSingleItem));
        }

        #endregion

        #region DeleteProductCondition Tests

        [Test]
        public void DeleteProductCondition_RemovesConditionFromRepository()
        {
            // Arrange
            AddConditionToDelete();
            Assert.That(_mockRepository.Conditions.Count, Is.EqualTo(ExpectedSingleItem),
                "Precondition: Repository should have exactly one condition before deletion");

            // Act
            _service.DeleteProductCondition(ConditionToDeleteTitle);

            // Assert
            Assert.That(_mockRepository.Conditions.Count, Is.EqualTo(ExpectedZeroItems));
        }

        #endregion

        #region Helper Methods

        private void AddTestConditions()
        {
            _mockRepository.Conditions.Add(new ProductCondition(FirstConditionId, NewConditionTitle, NewConditionDescription));
            _mockRepository.Conditions.Add(new ProductCondition(SecondConditionId, UsedConditionTitle, UsedConditionDescription));
        }

        private void AddConditionToDelete()
        {
            _mockRepository.Conditions.Add(new ProductCondition(FirstConditionId, ConditionToDeleteTitle, ConditionToDeleteDescription));
        }

        #endregion
    }
}