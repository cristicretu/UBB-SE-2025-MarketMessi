using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace DomainLayer.Domain
{
    public class AuctionProduct : Product
    {
        [JsonPropertyName("startAuctionDate")]
        public DateTime StartAuctionDate { get; set; }
        [JsonPropertyName("endAuctionDate")]
        public DateTime EndAuctionDate { get; set; }
        [JsonPropertyName("startingPrice")]
        public float StartingPrice { get; set; }
        [JsonPropertyName("currentPrice")]
        public float CurrentPrice { get; set; }

        [JsonPropertyName("bidHistory")]
        public List<Bid> BiddingHistory { get; set; }

        public AuctionProduct() : base()
        {
            BiddingHistory = new List<Bid>();
            Tags = new List<ProductTag>();
            Images = new List<Image>();
        }

        public AuctionProduct(int id, string title, string description, User seller, ProductCondition productCondition, ProductCategory productCategory,
            List<ProductTag> productTags, List<Image> images, DateTime startAuctionDate, DateTime endAuctionDate, float startingPrice)
        {
            this.Id = id;
            this.Description = description;
            this.Title = title;
            this.Seller = seller;
            this.Condition = productCondition;
            this.Category = productCategory;
            this.Tags = productTags;
            this.Seller = seller;
            this.Images = images;
            this.StartAuctionDate = startAuctionDate;
            this.EndAuctionDate = endAuctionDate;
            this.StartingPrice = startingPrice;
            this.CurrentPrice = startingPrice;
            this.BiddingHistory = new List<Bid>();
        }

        public void AddBid(Bid bid)
        {
            this.CurrentPrice = bid.Price;
            this.BiddingHistory.Add(bid);
        }
    }
}
