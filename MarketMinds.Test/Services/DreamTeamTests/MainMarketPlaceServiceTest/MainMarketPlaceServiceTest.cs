using System.Collections.Generic;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Services.DreamTeam.MainMarketplaceService;

namespace MarketMinds.Test.Services.DreamTeamTests.MainMarketPlaceServiceTest
{
    [TestFixture]
    public class MainMarketplaceServiceTest
    {
        private MainMarketplaceService mainMarketplaceService;
        private MainMarketplaceRepositoryMock mainMarketplaceRepositoryMock;

        private const int EXPECTED_ITEM_ID = 1;
        private const string EXPECTED_ITEM_NAME = "Test Item";
        private const string EXPECTED_ITEM_DESCRIPTION = "Test Description";
        private const float EXPECTED_ITEM_COST = 99.99f;
        private const int EXPECTED_SELLER_ID = 100;
        private const int EXPECTED_BUYER_ID = -1;

        [SetUp]
        public void Setup()
        {
            mainMarketplaceRepositoryMock = new MainMarketplaceRepositoryMock();
            mainMarketplaceService = new MainMarketplaceService(mainMarketplaceRepositoryMock);
        }

        [Test]
        public void TestGetAvailableItems_ReturnsItems()
        {
            var expectedItem = new UserNotSoldOrder
            {
                Id = EXPECTED_ITEM_ID,
                Name = EXPECTED_ITEM_NAME,
                Description = EXPECTED_ITEM_DESCRIPTION,
                Cost = EXPECTED_ITEM_COST,
                SellerId = EXPECTED_SELLER_ID,
                BuyerId = EXPECTED_BUYER_ID
            };
            mainMarketplaceRepositoryMock.AddAvailableItem(expectedItem);

            var retrievedItems = mainMarketplaceService.GetAvailableItems();

            Assert.That(retrievedItems, Is.Not.Null);
            Assert.That(retrievedItems.Count, Is.EqualTo(1));
            Assert.That(retrievedItems[0].Name, Is.EqualTo(EXPECTED_ITEM_NAME));
        }

        [Test]
        public void TestGetAvailableItems_NoItems_ReturnsEmptyList()
        {
            var retrievedItems = mainMarketplaceService.GetAvailableItems();

            Assert.That(retrievedItems, Is.Not.Null);
            Assert.That(retrievedItems.Count, Is.EqualTo(0));
        }
    }
}
