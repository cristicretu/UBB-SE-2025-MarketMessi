using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// TODO: Define or import ProductCondition, ProductCategory, User if they are needed by the server

namespace server.Models // Adjusted namespace to server.Models
{
    public abstract class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;
        
        // Foreign keys
        [Column("seller_id")]
        public int SellerId { get; set; }

        [Column("condition_id")]
        public int ConditionId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }
        
        // Navigation properties
        [ForeignKey("SellerId")]
        public User Seller { get; set; }

        [ForeignKey("ConditionId")]
        public Condition Condition { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
} 