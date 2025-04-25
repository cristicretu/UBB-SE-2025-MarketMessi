using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("BuyProductProductTags")]
    public class BuyProductProductTag
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("tag_id")]
        public int TagId { get; set; }

        // Navigation properties and Foreign Key relationships are configured in ApplicationDbContext

        public BuyProductProductTag() { }

        public BuyProductProductTag(int id, int productId, int tagId)
        {
            Id = id;
            ProductId = productId;
            TagId = tagId;
        }
    }
}