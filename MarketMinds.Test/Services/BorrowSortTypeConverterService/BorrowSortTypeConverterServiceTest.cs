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

        // Sort tag constants
        private const string SellerRatingAscTag = "SellerRatingAsc";
        private const string SellerRatingDescTag = "SellerRatingDesc";
        private const string DailyRateAscTag = "DailyRateAsc";
        private const string DailyRateDescTag = "DailyRateDesc";
        private const string StartDateAscTag = "StartDateAsc";
        private const string StartDateDescTag = "StartDateDesc";
        private const string InvalidTag = "InvalidSortTag";
        private const string EmptyTag = "";

        // Display title constants
        private const string SellerRatingTitle = "Seller Rating";
        private const string SellerRatingField = "SellerRating";
        private const string DailyRateTitle = "Daily Rate";
        private const string DailyRateField = "DailyRate";
        private const string StartDateTitle = "Start Date";
        private const string StartDateField = "StartDate";

        [SetUp]
        public void Setup()
        {
            _converter = new BorrowSortTypeConverterService();
        }

        #region SellerRating Tests

        [Test]
        public void Convert_SellerRatingAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(SellerRatingAscTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_SellerRatingAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(SellerRatingAscTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(SellerRatingTitle));
        }

        [Test]
        public void Convert_SellerRatingAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(SellerRatingAscTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(SellerRatingField));
        }

        [Test]
        public void Convert_SellerRatingAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(SellerRatingAscTag);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_SellerRatingDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(SellerRatingDescTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_SellerRatingDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(SellerRatingDescTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(SellerRatingTitle));
        }

        [Test]
        public void Convert_SellerRatingDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(SellerRatingDescTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(SellerRatingField));
        }

        [Test]
        public void Convert_SellerRatingDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(SellerRatingDescTag);
            Assert.That(result.IsAscending, Is.False);
        }

        #endregion

        #region DailyRate Tests

        [Test]
        public void Convert_DailyRateAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(DailyRateAscTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_DailyRateAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(DailyRateAscTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(DailyRateTitle));
        }

        [Test]
        public void Convert_DailyRateAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(DailyRateAscTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(DailyRateField));
        }

        [Test]
        public void Convert_DailyRateAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(DailyRateAscTag);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_DailyRateDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(DailyRateDescTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_DailyRateDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(DailyRateDescTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(DailyRateTitle));
        }

        [Test]
        public void Convert_DailyRateDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(DailyRateDescTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(DailyRateField));
        }

        [Test]
        public void Convert_DailyRateDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(DailyRateDescTag);
            Assert.That(result.IsAscending, Is.False);
        }

        #endregion

        #region StartDate Tests

        [Test]
        public void Convert_StartDateAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(StartDateAscTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_StartDateAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(StartDateAscTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(StartDateTitle));
        }

        [Test]
        public void Convert_StartDateAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(StartDateAscTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(StartDateField));
        }

        [Test]
        public void Convert_StartDateAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(StartDateAscTag);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_StartDateDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(StartDateDescTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_StartDateDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(StartDateDescTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(StartDateTitle));
        }

        [Test]
        public void Convert_StartDateDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(StartDateDescTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(StartDateField));
        }

        [Test]
        public void Convert_StartDateDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(StartDateDescTag);
            Assert.That(result.IsAscending, Is.False);
        }

        #endregion

        #region Invalid Input Tests

        [Test]
        public void Convert_InvalidSortTag_ReturnsNull()
        {
            var result = _converter.Convert(InvalidTag);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_EmptyString_ReturnsNull()
        {
            var result = _converter.Convert(EmptyTag);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_NullString_ReturnsNull()
        {
            var result = _converter.Convert(null);
            Assert.That(result, Is.Null);
        }

        #endregion
    }
}