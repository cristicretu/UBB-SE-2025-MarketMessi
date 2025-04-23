namespace server.Models
{
    public class BorrowProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int ProductId { get; set; }
        
        // Navigation property
        public BorrowProduct Product { get; set; }
        
        public BorrowProductImage() { }
        
        public BorrowProductImage(int id, string url, int productId)
        {
            Id = id;
            Url = url;
            ProductId = productId;
        }
    }
} 