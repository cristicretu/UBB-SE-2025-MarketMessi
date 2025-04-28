using System;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Repository;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using MarketMinds.Services.ProductTagService;
using MarketMinds.Repositories;

namespace MarketMinds.Services.AuctionProductsService
{
    public class AuctionProductsRepository : IAuctionProductsService
    {
        private readonly AuctionProductsRepository auctionProductsRepository;

        private const int NULL_BID_AMOUNT = 0;
        private const int MAX_AUCTION_TIME = 5;

        public AuctionProductsRepository(AuctionProductsRepository auctionProductsRepository)
        {
            this.auctionProductsRepository = auctionProductsRepository;
        }

        public void CreateListing(Product product)
        {
            auctionProductsRepository.CreateListing(product);
        }

        public void PlaceBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            auctionProductsRepository.PlaceBid(auction, bidder, bidAmount);
        }

        public void ConcludeAuction(AuctionProduct auction)
        {
            auctionProductsRepository.ConcludeAuction(auction);
        }

        public string GetTimeLeft(AuctionProduct auction)
        {
            var timeLeft = auction.EndTime - DateTime.Now;
            return timeLeft > TimeSpan.Zero ? timeLeft.ToString(@"dd\:hh\:mm\:ss") : "Auction Ended";
        }

        public bool IsAuctionEnded(AuctionProduct auction)
        {
            return DateTime.Now >= auction.EndTime;
        }

        public void ValidateBid(AuctionProduct auction, User bidder, float bidAmount)
        {
            auctionProductsRepository.ValidateBid(auction, bidder, bidAmount);
        }

        public void ExtendAuctionTime(AuctionProduct auction)
        {
            auctionProductsRepository.ExtendAuctionTime(auction);
        }

        public List<AuctionProduct> GetProducts()
        {
            return auctionProductsRepository.GetProducts();
        }

        public AuctionProduct GetProductById(int id)
        {
            return auctionProductsRepository.GetProductById(id);
        }
    }
}
