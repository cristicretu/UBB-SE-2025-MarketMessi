using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.DreamTeam.MainMarketplaceService;

/// <summary>
/// This interface defines the contract for the Main Marketplace Service.
/// </summary>
public interface IMainMarketplaceService
{
    /// <summary>
    /// Retrieves a list of all available items currently listed in the marketplace.
    /// These are items that have been posted for sale by sellers but have not yet been sold.
    /// </summary>
    /// <returns>A List of UserNotSoldOrder objects representing available marketplace items.</returns>
    List<UserNotSoldOrder> GetAvailableItems();
}
