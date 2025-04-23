using DomainLayer.Domain;
using MarketMinds.Services;
using NUnit.Framework;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class SortTypeConverterServiceTest
    {
        // Constants for sort tags
        private const string PriceAscTag = "PriceAsc";
        private const string PriceDescTag = "PriceDesc";
        private const string InvalidSortTag = "InvalidSortTag";
        private const string EmptyTag = "";

        // Constants for field titles
        private const string PriceFieldTitle = "Price";

        private SortTypeConverterService _converter;

        [SetUp]
        public void Setup()
        {
            _converter = new SortTypeConverterService();
        }

        [Test]
        public void Convert_PriceAsc_ReturnsNonNullResult()
        {
            // Arrange
            string sortTag = PriceAscTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_PriceAsc_ReturnsCorrectExternalTitle()
        {
            // Arrange
            string sortTag = PriceAscTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(PriceFieldTitle));
        }

        [Test]
        public void Convert_PriceAsc_ReturnsCorrectInternalTitle()
        {
            // Arrange
            string sortTag = PriceAscTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(PriceFieldTitle));
        }

        [Test]
        public void Convert_PriceAsc_ReturnsAscendingOrder()
        {
            // Arrange
            string sortTag = PriceAscTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_PriceDesc_ReturnsNonNullResult()
        {
            // Arrange
            string sortTag = PriceDescTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_PriceDesc_ReturnsCorrectExternalTitle()
        {
            // Arrange
            string sortTag = PriceDescTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(PriceFieldTitle));
        }

        [Test]
        public void Convert_PriceDesc_ReturnsCorrectInternalTitle()
        {
            // Arrange
            string sortTag = PriceDescTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(PriceFieldTitle));
        }

        [Test]
        public void Convert_PriceDesc_ReturnsDescendingOrder()
        {
            // Arrange
            string sortTag = PriceDescTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.IsAscending, Is.False);
        }

        [Test]
        public void Convert_InvalidSortTag_ReturnsNull()
        {
            // Arrange
            string sortTag = InvalidSortTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_EmptyString_ReturnsNull()
        {
            // Arrange
            string sortTag = EmptyTag;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_NullString_ReturnsNull()
        {
            // Arrange
            string sortTag = null;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}