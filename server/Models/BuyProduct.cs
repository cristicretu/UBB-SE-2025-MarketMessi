using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("BuyProducts")]
    public class BuyProduct
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

        [Column("price")]
        public double Price { get; set; }

        // Navigation properties
        [ForeignKey("SellerId")]
        public User Seller { get; set; } = null!;

        [ForeignKey("ConditionId")]
        public Condition? Condition { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<BuyProductImage> Images { get; set; } = new List<BuyProductImage>();
        public ICollection<BuyProductProductTag> ProductTags { get; set; } = new List<BuyProductProductTag>();

        public BuyProduct()
        {
            Images = new List<BuyProductImage>();
            ProductTags = new List<BuyProductProductTag>();
        }

        public BuyProduct(string title, string? description, int sellerId, int? conditionId,
            int? categoryId, double price)
        {
            Title = title;
            Description = description;
            SellerId = sellerId;
            ConditionId = conditionId;
            CategoryId = categoryId;
            Price = price;
            Images = new List<BuyProductImage>();
            ProductTags = new List<BuyProductProductTag>();
        }
    }
}