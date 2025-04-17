using System;
using System.Collections.Generic;

// TODO: Define or import User, ProductCondition, ProductCategory, Bid if they are needed by the server

namespace server.Models // Adjusted namespace to server.Models
{
    public class AuctionProduct : Product
    {
        public DateTime StartAuctionDate { get; set; }
        public DateTime EndAuctionDate { get; set; }
        public float StartingPrice { get; set; }
        public float CurrentPrice { get; set; }

        // Navigation properties
        public ICollection<Bid> BidHistory { get; set; }
        public ICollection<AuctionProductImage> Images { get; set; }
        public ICollection<AuctionProductProductTag> ProductTags { get; set; }
        
        // Legacy properties for backward compatibility with the repository
        public List<ProductTag> Tags { get; set; }
        public List<Image> LegacyImages { get; set; }

        public AuctionProduct()
        {
            BidHistory = new List<Bid>();
            Images = new List<AuctionProductImage>();
            ProductTags = new List<AuctionProductProductTag>();
            Tags = new List<ProductTag>();
            LegacyImages = new List<Image>();
        }

        // Constructor for new Entity Framework implementation
        public AuctionProduct(int id, string title, string description, User seller, 
            ProductCondition condition, ProductCategory category, DateTime startAuctionDate, 
            DateTime endAuctionDate, float startingPrice)
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            Condition = condition;
            Category = category;
            StartAuctionDate = startAuctionDate;
            EndAuctionDate = endAuctionDate;
            StartingPrice = startingPrice;
            CurrentPrice = startingPrice;
            BidHistory = new List<Bid>();
            Images = new List<AuctionProductImage>();
            ProductTags = new List<AuctionProductProductTag>();
            Tags = new List<ProductTag>();
            LegacyImages = new List<Image>();
        }

        // Legacy constructor for the repository
        public AuctionProduct(int id, string title, string description, User seller, 
            ProductCondition condition, ProductCategory category, List<ProductTag> tags, 
            List<Image> images, DateTime startAuctionDate, DateTime endAuctionDate, float startingPrice)
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            Condition = condition;
            Category = category;
            StartAuctionDate = startAuctionDate;
            EndAuctionDate = endAuctionDate;
            StartingPrice = startingPrice;
            CurrentPrice = startingPrice;
            BidHistory = new List<Bid>();
            Images = new List<AuctionProductImage>();
            ProductTags = new List<AuctionProductProductTag>();
            Tags = tags;
            LegacyImages = images;
        }

        public void AddBid(Bid bid)
        {
            CurrentPrice = bid.Price;
            BidHistory.Add(bid);
        }
    }
} 