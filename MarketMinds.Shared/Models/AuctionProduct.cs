using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MarketMinds.Shared.Models // Adjusted namespace to server.Models
{
    [Table("AuctionProducts")]
    public class AuctionProduct : Product
    {
        [Column("start_datetime")]
        public DateTime StartTime { get; set; }

        [Column("end_datetime")]
        public DateTime EndTime { get; set; }

        [Column("starting_price")]
        public double StartPrice { get; set; }

        [NotMapped]
        public double StartingPrice => StartPrice;

        [Column("current_price")]
        public double CurrentPrice { get; set; }

        public ICollection<Bid> Bids { get; set; } = new List<Bid>();

        [NotMapped]
        public IEnumerable<Bid> BidHistory => Bids?.OrderByDescending(b => b.Timestamp) ?? Enumerable.Empty<Bid>();

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        [NotMapped]
        public List<Image> NonMappedImages { get; set; } = new List<Image>();

        public AuctionProduct() : base()
        {
            NonMappedImages = new List<Image>();
        }

        public AuctionProduct(string title, string? description, int sellerId, int? conditionId,
                           int? categoryId, DateTime startTime,
                           DateTime endTime, double startPrice) : base()
        {
            Title = title;
            Description = description ?? string.Empty;
            SellerId = sellerId;
            ConditionId = conditionId ?? 0;
            CategoryId = categoryId ?? 0;
            StartTime = startTime;
            EndTime = endTime;
            StartPrice = startPrice;
            CurrentPrice = startPrice;
            Bids = new List<Bid>();
            Images = new List<ProductImage>();
            NonMappedImages = new List<Image>();
        }

        public AuctionProduct(int id, string title, string description, User seller,
                             Condition condition, Category category, List<ProductTag> productTags,
                             List<Image> images, DateTime startTime, DateTime endTime, double startPrice) : base()
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            Condition = condition;
            Category = category;
            StartTime = startTime;
            EndTime = endTime;
            StartPrice = startPrice;
            CurrentPrice = startPrice;
            Tags = productTags ?? new List<ProductTag>();
            NonMappedImages = images ?? new List<Image>();
        }

        public void AddBid(Bid bid)
        {
            Bids.Add(bid);
            if (bid.Price > CurrentPrice)
            {
                CurrentPrice = bid.Price;
            }
        }
    }
}