using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Repositories.MainMarketplaceRepository;

/// <summary>
/// This interface defines the contract for the Main Marketplace Repository.
/// </summary>
public interface IMainMarketplaceRepository
{
    /// <summary>
    /// Retrieves a list of all available items currently listed in the marketplace.
    /// </summary>
    /// <returns>A list of UserNotSoldOrder objects representing products available for purchase in the marketplace.</returns>
    List<UserNotSoldOrder> GetAvailableItems();
}
