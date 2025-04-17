using System;

// TODO: Define or import ProductCondition, ProductCategory, User if they are needed by the server

namespace server.Models // Adjusted namespace to server.Models
{
    public abstract class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Foreign keys
        public int SellerId { get; set; }
        public int ConditionId { get; set; }
        public int CategoryId { get; set; }
        
        // Navigation properties
        public User Seller { get; set; }
        public ProductCondition Condition { get; set; }
        public ProductCategory Category { get; set; }
    }
} 