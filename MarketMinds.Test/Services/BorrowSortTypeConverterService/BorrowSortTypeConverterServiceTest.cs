using DomainLayer.Domain;
using MarketMinds.Services;
using NUnit.Framework;
using MarketMinds.Services.BorrowSortTypeConverterService;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class BorrowSortTypeConverterServiceTest
    {
        private BorrowSortTypeConverterService _converter;

        [SetUp]
        public void Setup()
        {
            _converter = new BorrowSortTypeConverterService();
        }

        [Test]
        public void Convert_SellerRatingAsc_ReturnsCorrectSortType()
        {
            string sortTag = "SellerRatingAsc";

            var result = _converter.Convert(sortTag);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo("Seller Rating"));
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo("SellerRating"));
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_SellerRatingDesc_ReturnsCorrectSortType()
        {
            string sortTag = "SellerRatingDesc";
            var result = _converter.Convert(sortTag);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo("Seller Rating"));
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo("SellerRating"));
            Assert.That(result.IsAscending, Is.False);
        }

        [Test]
        public void Convert_DailyRateAsc_ReturnsCorrectSortType()
        {
            string sortTag = "DailyRateAsc";
            var result = _converter.Convert(sortTag);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo("Daily Rate"));
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo("DailyRate"));
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_DailyRateDesc_ReturnsCorrectSortType()
        {
            string sortTag = "DailyRateDesc";
            var result = _converter.Convert(sortTag);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo("Daily Rate"));
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo("DailyRate"));
            Assert.That(result.IsAscending, Is.False);
        }

        [Test]
        public void Convert_StartDateAsc_ReturnsCorrectSortType()
        {
            string sortTag = "StartDateAsc";
            var result = _converter.Convert(sortTag);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo("Start Date"));
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo("StartDate"));
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_StartDateDesc_ReturnsCorrectSortType()
        {
            string sortTag = "StartDateDesc";
            var result = _converter.Convert(sortTag);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo("Start Date"));
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo("StartDate"));
            Assert.That(result.IsAscending, Is.False);
        }

        [Test]
        public void Convert_InvalidSortTag_ReturnsNull()
        {
            string sortTag = "InvalidSortTag";
            var result = _converter.Convert(sortTag);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_EmptyString_ReturnsNull()
        {
            string sortTag = "";
            var result = _converter.Convert(sortTag);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_NullString_ReturnsNull()
        {
            string sortTag = null;
            var result = _converter.Convert(sortTag);

            Assert.That(result, Is.Null);
        }
    }
}