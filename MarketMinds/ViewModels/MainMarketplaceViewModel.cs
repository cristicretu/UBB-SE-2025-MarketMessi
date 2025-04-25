using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Services.DreamTeam.MainMarketplaceService;

namespace MarketMinds.ViewModels;

public class MainMarketplaceViewModel
{
    private readonly IMainMarketplaceService mainMarketplaceService;

    public MainMarketplaceViewModel(IMainMarketplaceService mainMarketplaceService)
    {
        this.mainMarketplaceService = mainMarketplaceService;
    }

    public List<UserNotSoldOrder> GetAvailableItems()
    {
        return mainMarketplaceService.GetAvailableItems();
    }
}
