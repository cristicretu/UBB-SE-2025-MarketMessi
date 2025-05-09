using System;
using System.Linq;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.ProductTagService;

namespace MarketMinds.Shared.Services.AuctionProductsService
{
    public class AuctionProductsService : IAuctionProductsService, IProductService
    {
        private readonly AuctionProductsProxyRepository auctionProductsRepository;

        private const int NULL_BID_AMOUNT = 0;
        private const int MAX_AUCTION_TIME = 5;
        private const int NOCOUNT = 0;

        public AuctionProductsService(AuctionProductsProxyRepository auctionProductsRepository)
        {
            this.auctionProductsRepository = auctionProductsRepository;
        }

        public void CreateListing(Product product)
        {
            if (!(product is AuctionProduct auctionProduct))
            {
                throw new ArgumentException("Product must be an AuctionProduct.", nameof(product));
            }
            
            if (auctionProduct.StartTime == default(DateTime))
            {
                auctionProduct.StartTime = DateTime.Now;
            }
            
            if (auctionProduct.EndTime == default(DateTime))
            {
                auctionProduct.EndTime = DateTime.Now.AddDays(7);
            }
            
            if (auctionProduct.StartPrice <= 0 && auctionProduct.CurrentPrice > 0)
            {
                auctionProduct.StartPrice = auctionProduct.CurrentPrice;
            }
            
            auctionProductsRepository.CreateListing(auctionProduct);
        }

        public void PlaceBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            try
            {
                ValidateBid(auction, bidder, bidAmount);
                
                var bid = new Bid(bidder.Id, auction.Id, bidAmount)
                {
                    Product = auction,
                    Bidder = bidder
                };
                
                bidder.Balance -= bidAmount;
                
                auction.AddBid(bid);
                auction.CurrentPrice = bidAmount;
                
                ExtendAuctionTime(auction);
                
                auctionProductsRepository.PlaceBid(auction, bidder, bidAmount);
            }
            catch (Exception bidPlacementException)
            {
                throw new InvalidOperationException("Failed to place bid", bidPlacementException);
            }
        }

        public void ConcludeAuction(AuctionProduct auction)
        {
            if (auction.Id == 0)
            {
                throw new ArgumentException("Auction Product ID must be set for delete.", nameof(auction.Id));
            }
            
            auctionProductsRepository.ConcludeAuction(auction);
        }

        public string GetTimeLeft(AuctionProduct auction)
        {
            var timeLeft = auction.EndTime - DateTime.Now;
            string result = timeLeft > TimeSpan.Zero ? timeLeft.ToString(@"dd\:hh\:mm\:ss") : "Auction Ended";
            return result;
        }

        public bool IsAuctionEnded(AuctionProduct auction)
        {
            bool isEnded = DateTime.Now >= auction.EndTime;
            return isEnded;
        }

        public void ValidateBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            double minimumBid = auction.Bids.Count == NULL_BID_AMOUNT ? auction.StartPrice : auction.CurrentPrice + 1;

            if (bidAmount < minimumBid)
            {
                throw new Exception($"Bid must be at least ${minimumBid}");
            }

            if (bidAmount > bidder.Balance)
            {
                throw new Exception("Insufficient balance");
            }

            if (DateTime.Now > auction.EndTime)
            {
                throw new Exception("Auction already ended");
            }
        }

        public void ExtendAuctionTime(AuctionProduct auction)
        {
            var timeRemaining = auction.EndTime - DateTime.Now;

            if (timeRemaining.TotalMinutes < MAX_AUCTION_TIME)
            {
                var oldEndTime = auction.EndTime;
                auction.EndTime = DateTime.Now.AddMinutes(MAX_AUCTION_TIME);
            }
        }

        public List<AuctionProduct> GetProducts()
        {
            try
            {
                return auctionProductsRepository.GetProducts();
            }
            catch (Exception exception)
            {
                if (exception.InnerException != null)
                {
                    throw new Exception($"Inner exception: {exception.InnerException.Message}", exception.InnerException);
                }
            }
        }

        public AuctionProduct GetProductById(int id)
        {
            try
            {
                var product = auctionProductsRepository.GetProductById(id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Auction product with ID {id} not found.");
                }
                return product;
            }
            catch (Exception exception)
            {
                throw new KeyNotFoundException($"Auction product with ID {id} not found: {exception.Message}");
            }
        }

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            List<AuctionProduct> products = GetProducts();
            List<Product> productResultSet = new List<Product>();
            foreach (Product product in products)
            {
                bool matchesConditions = selectedConditions == null || selectedConditions.Count == NOCOUNT || selectedConditions.Any(condition => condition.Id == product.Condition.Id);
                bool matchesCategories = selectedCategories == null || selectedCategories.Count == NOCOUNT || selectedCategories.Any(category => category.Id == product.Category.Id);
                bool matchesTags = selectedTags == null || selectedTags.Count == NOCOUNT || selectedTags.Any(tag => product.Tags.Any(productTag => productTag.Id == tag.Id));
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
                        product =>
                        {
                            var property = product?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return property?.GetValue(product);
                        }).ToList();
                }
                else
                {
                    productResultSet = productResultSet.OrderByDescending(
                        product =>
                        {
                            var property = product?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return property?.GetValue(product);
                        }).ToList();
                }
            }
            return productResultSet;
        }
    }
}
