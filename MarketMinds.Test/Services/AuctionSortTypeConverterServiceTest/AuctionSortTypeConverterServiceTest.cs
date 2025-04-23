using DomainLayer.Domain;
using MarketMinds.Services;
using NUnit.Framework;
using MarketMinds.Services.AuctionSortTypeConverterService;

namespace MarketMinds.Test.Services.AuctionSortTypeConverterServiceTest
{
    [TestFixture]
    public class AuctionSortTypeConverterServiceTest
    {
        private AuctionSortTypeConverterService _converter;

        // Sort tag constants
        private const string SellerRatingAscTag = "SellerRatingAsc";
        private const string SellerRatingDescTag = "SellerRatingDesc";
        private const string StartingPriceAscTag = "StartingPriceAsc";
        private const string StartingPriceDescTag = "StartingPriceDesc";
        private const string CurrentPriceAscTag = "CurrentPriceAsc";
        private const string CurrentPriceDescTag = "CurrentPriceDesc";
        private const string InvalidTag = "InvalidSortTag";
        private const string EmptyTag = "";

        // Display title constants
        private const string SellerRatingTitle = "Seller Rating";
        private const string SellerRatingField = "SellerRating";
        private const string StartingPriceTitle = "Starting Price";
        private const string StartingPriceField = "StartingPrice";
        private const string CurrentPriceTitle = "Current Price";
        private const string CurrentPriceField = "CurrentPrice";

        [SetUp]
        public void Setup()
        {
            _converter = new AuctionSortTypeConverterService();
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

        #region StartingPrice Tests

        [Test]
        public void Convert_StartingPriceAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(StartingPriceAscTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_StartingPriceAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(StartingPriceAscTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(StartingPriceTitle));
        }

        [Test]
        public void Convert_StartingPriceAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(StartingPriceAscTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(StartingPriceField));
        }

        [Test]
        public void Convert_StartingPriceAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(StartingPriceAscTag);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_StartingPriceDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(StartingPriceDescTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_StartingPriceDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(StartingPriceDescTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(StartingPriceTitle));
        }

        [Test]
        public void Convert_StartingPriceDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(StartingPriceDescTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(StartingPriceField));
        }

        [Test]
        public void Convert_StartingPriceDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(StartingPriceDescTag);
            Assert.That(result.IsAscending, Is.False);
        }

        #endregion

        #region CurrentPrice Tests

        [Test]
        public void Convert_CurrentPriceAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(CurrentPriceAscTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_CurrentPriceAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(CurrentPriceAscTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(CurrentPriceTitle));
        }

        [Test]
        public void Convert_CurrentPriceAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(CurrentPriceAscTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(CurrentPriceField));
        }

        [Test]
        public void Convert_CurrentPriceAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(CurrentPriceAscTag);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_CurrentPriceDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(CurrentPriceDescTag);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_CurrentPriceDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(CurrentPriceDescTag);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(CurrentPriceTitle));
        }

        [Test]
        public void Convert_CurrentPriceDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(CurrentPriceDescTag);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(CurrentPriceField));
        }

        [Test]
        public void Convert_CurrentPriceDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(CurrentPriceDescTag);
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