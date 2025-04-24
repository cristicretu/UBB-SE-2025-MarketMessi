using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DomainLayer.Domain
{
    public class Review
    {
        // The review doesn't take into account the product for which the review has been made,
        // it can be mentioned in the description, and images
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public List<Image> Images { get; set; } = new List<Image>();

        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        [JsonPropertyName("sellerId")]
        public int SellerId { get; set; }

        [JsonPropertyName("buyerId")]
        public int BuyerId { get; set; }

        [JsonPropertyName("reviewImages")]
        [JsonIgnore] // Ignore this property during serialization/deserialization
        public ICollection<object> ReviewImages { get; set; } = new List<object>();

        // New properties for usernames
        public string BuyerUsername { get; set; } = string.Empty;

        public string SellerUsername { get; set; } = string.Empty;

        // public int productId { get; set; }
        public Review(int id, string description, List<Image> images, double rating, int sellerId, int buyerId)
        {
            this.Id = id;
            this.Description = description;
            this.Images = images ?? new List<Image>();
            this.Rating = rating;
            this.SellerId = sellerId;
            this.BuyerId = buyerId;
            this.BuyerUsername = string.Empty;
            this.SellerUsername = string.Empty;
        }

        // Default constructor for JSON deserialization
        public Review()
        {
            Images = new List<Image>();
        }
    }
}
