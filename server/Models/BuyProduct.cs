using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Models
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
        public User? Seller { get; set; }

        [ForeignKey("ConditionId")]
        public Condition? Condition { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<BuyProductImage> Images { get; set; } = new List<BuyProductImage>();
        public ICollection<BuyProductProductTag> ProductTags { get; set; } = new List<BuyProductProductTag>();

        [NotMapped]
        public List<ProductTag> Tags { get; set; }

        [NotMapped]
        public List<Image> NonMappedImages { get; set; }

        public BuyProduct() : base()
        {
            Price = 0;
            Tags = new List<ProductTag>();
            NonMappedImages = new List<Image>();
        }

        public BuyProduct(int id, string title, string description, User seller, Condition productCondition, Category productCategory,
            List<ProductTag> productTags, List<Image> images, double price)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.Seller = seller;
            this.Condition = productCondition;
            this.Category = productCategory;
            this.Tags = productTags ?? new List<ProductTag>();
            this.NonMappedImages = images ?? new List<Image>();
            this.Price = price;
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
            NonMappedImages = new List<Image>();
            Tags = new List<ProductTag>();
        }
    }
}
