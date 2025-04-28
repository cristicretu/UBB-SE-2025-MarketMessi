using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models // Adjusted namespace to server.Models
{
    [Table("AuctionProducts")]
    public class AuctionProduct
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("seller_id")]
        public int SellerId { get; set; }

        [Column("condition_id")]
        public int? ConditionId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("start_datetime")]
        public DateTime StartTime { get; set; }

        [Column("end_datetime")]
        public DateTime EndTime { get; set; }

        [Column("starting_price")]
        public double StartPrice { get; set; }

        [Column("current_price")]
        public double CurrentPrice { get; set; }

        // Navigation properties
        [ForeignKey("SellerId")]
        public User? Seller { get; set; }

        [ForeignKey("ConditionId")]
        public Condition? Condition { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<Bid> Bids { get; set; } = new List<Bid>();

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public AuctionProduct()
        {
        }

        public AuctionProduct(string title, string? description, int sellerId, int? conditionId,
                             int? categoryId, DateTime startTime,
                             DateTime endTime, double startPrice)
        {
            Title = title;
            Description = description;
            SellerId = sellerId;
            ConditionId = conditionId;
            CategoryId = categoryId;
            StartTime = startTime;
            EndTime = endTime;
            StartPrice = startPrice;
            CurrentPrice = startPrice;
            Bids = new List<Bid>();
            Images = new List<ProductImage>();
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