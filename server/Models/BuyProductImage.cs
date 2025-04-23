namespace server.Models
{
    public class BuyProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int ProductId { get; set; }
        
        // Navigation property
        public BuyProduct Product { get; set; }
        
        public BuyProductImage() { }
        
        public BuyProductImage(int id, string url, int productId)
        {
            Id = id;
            Url = url;
            ProductId = productId;
        }
    }
} 