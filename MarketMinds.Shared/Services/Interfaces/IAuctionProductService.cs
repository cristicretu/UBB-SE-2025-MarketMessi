using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.Interfaces
{
    public interface IAuctionProductService
    {
        Task<List<AuctionProduct>> GetAllAuctionProductsAsync();
        Task<AuctionProduct> GetAuctionProductByIdAsync(int id);
        Task<bool> CreateAuctionProductAsync(AuctionProduct auctionProduct);
        Task<bool> PlaceBidAsync(int auctionId, int bidderId, double bidAmount);
        Task<bool> UpdateAuctionProductAsync(AuctionProduct auctionProduct);
        Task<bool> DeleteAuctionProductAsync(int id);
    }
} 