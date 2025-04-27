using System.Collections.Generic;
using DomainLayer.Domain;
using MarketMinds.Repositories.MainMarketplaceRepository;

namespace MarketMinds.Test.Services.DreamTeamTests.MainMarketPlaceServiceTest
{
    public class MainMarketplaceRepositoryMock : IMainMarketplaceRepository
    {
        private List<UserNotSoldOrder> availableItems = new List<UserNotSoldOrder>();

        public List<UserNotSoldOrder> GetAvailableItems()
        {
            return availableItems;
        }

        // Helper method for tests
        public void AddAvailableItem(UserNotSoldOrder item)
        {
            availableItems.Add(item);
        }
    }
}
