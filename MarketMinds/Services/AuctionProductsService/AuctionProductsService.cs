using System;
using System.Linq;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Repository;
using MarketMinds.Services.ProductTagService;

namespace MarketMinds.Services.AuctionProductsService
{
    public class AuctionProductsService : IAuctionProductsService, IProductService
    {
        private readonly AuctionProductsRepository auctionProductsRepository;

        private const int NULL_BID_AMOUNT = 0;
        private const int MAX_AUCTION_TIME = 5;
        private const int NOCOUNT = 0;

        public AuctionProductsService(AuctionProductsRepository auctionProductsRepository)
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

        public void ValidateBid(AuctionProduct auction, User bidder, double bidAmount)
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

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            List<AuctionProduct> products = GetProducts();
            List<Product> productResultSet = new List<Product>();
            foreach (Product product in products)
            {
                bool matchesConditions = selectedConditions == null || selectedConditions.Count == NOCOUNT || selectedConditions.Any(c => c.Id == product.Condition.Id);
                bool matchesCategories = selectedCategories == null || selectedCategories.Count == NOCOUNT || selectedCategories.Any(c => c.Id == product.Category.Id);
                bool matchesTags = selectedTags == null || selectedTags.Count == NOCOUNT || selectedTags.Any(t => product.Tags.Any(pt => pt.Id == t.Id));
                bool matchesSearchQuery = string.IsNullOrEmpty(searchQuery) || product.Title.ToLower().Contains(searchQuery.ToLower());

                if (matchesConditions && matchesCategories && matchesTags && matchesSearchQuery)
                {
                    productResultSet.Add(product);
                }
            }

            if (sortCondition != null)
            {
                if (sortCondition.IsAscending)
                {
                    productResultSet = productResultSet.OrderBy(
                        p =>
                        {
                            var prop = p?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return prop?.GetValue(p);
                        }).ToList();
                }
                else
                {
                    productResultSet = productResultSet.OrderByDescending(
                        p =>
                        {
                            var prop = p?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return prop?.GetValue(p);
                        }).ToList();
                }
            }
            return productResultSet;
        }
    }
}
