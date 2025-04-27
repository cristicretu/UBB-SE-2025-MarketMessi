using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories.MainMarketplaceRepository;

namespace MarketMinds.Services.DreamTeam.MainMarketplaceService;

public class MainMarketplaceService : IMainMarketplaceService
{
    private readonly IMainMarketplaceRepository mainMarketplaceRepository;

    public MainMarketplaceService(IMainMarketplaceRepository mainMarketplaceRepository)
    {
        this.mainMarketplaceRepository = mainMarketplaceRepository;
    }

    public List<UserNotSoldOrder> GetAvailableItems()
    {
        try
        {
            return mainMarketplaceRepository.GetAvailableItems();
        }
        catch (Exception exception)
        {
            return new List<UserNotSoldOrder>();
        }
    }
}
