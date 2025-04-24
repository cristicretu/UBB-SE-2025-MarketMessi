namespace server.Models
{
    public class BuyProductProductTag
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }
        
        // Navigation properties
        public BuyProduct Product { get; set; }
        public ProductTag Tag { get; set; }
        
        public BuyProductProductTag() { }
        
        public BuyProductProductTag(int id, int productId, int tagId)
        {
            Id = id;
            ProductId = productId;
            TagId = tagId;
        }
    }
} 