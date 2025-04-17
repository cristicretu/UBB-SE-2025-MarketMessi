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

        public List<Bid> BidHistory { get; set; } // Uncommented

        public AuctionProduct(int id, string title, string description, User seller, ProductCondition productCondition, ProductCategory productCategory,
            List<ProductTag> productTags, List<Image> images, DateTime startAuctionDate, DateTime endAuctionDate, float startingPrice)
        {
            this.Id = id;
            this.Description = description;
            this.Title = title;
            this.Seller = seller; // Uncommented
            this.Condition = productCondition; // Uncommented
            this.Category = productCategory; // Uncommented
            this.Tags = productTags;
            // this.Seller = seller; // Duplicate line from original - Removed
            this.Images = images;
            this.StartAuctionDate = startAuctionDate;
            this.EndAuctionDate = endAuctionDate;
            this.StartingPrice = startingPrice;
            this.CurrentPrice = startingPrice;
            this.BidHistory = new List<Bid>(); // Uncommented
        }

        // Uncommented - definition now exists
        public void AddBid(Bid bid)
        {
            this.CurrentPrice = bid.Price;
            this.BidHistory.Add(bid);
        }
        
        // Default constructor for framework needs
        public AuctionProduct() { }
    }
} 