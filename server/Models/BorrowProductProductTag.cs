namespace server.Models
{
    public class BorrowProductProductTag
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }
        
        // Navigation properties
        public BorrowProduct Product { get; set; }
        public ProductTag Tag { get; set; }
        
        public BorrowProductProductTag() { }
        
        public BorrowProductProductTag(int id, int productId, int tagId)
        {
            Id = id;
            ProductId = productId;
            TagId = tagId;
        }
    }
} 