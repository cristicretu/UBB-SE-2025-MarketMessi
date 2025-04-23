using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Repositories.MainMarketplaceRepository
{
    public interface IMainMarketplaceRepository
    {
        List<UserNotSoldOrder> GetAvailableItems();
    }
}
