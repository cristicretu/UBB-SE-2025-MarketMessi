using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.DreamTeam.MainMarketplaceService;

public interface IMainMarketplaceService
{
    List<UserNotSoldOrder> GetAvailableItems();
}
