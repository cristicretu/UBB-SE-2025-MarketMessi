using System;

namespace server.Models
{
    public class AuctionProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int ProductId { get; set; }
        
        // Navigation property
        public AuctionProduct Product { get; set; }
        
        public AuctionProductImage() { }
        
        public AuctionProductImage(int id, string url, int productId)
        {
            Id = id;
            Url = url;
            ProductId = productId;
        }
    }
} 