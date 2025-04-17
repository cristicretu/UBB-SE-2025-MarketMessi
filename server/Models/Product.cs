using System;
using System.Collections.Generic;

// TODO: Define or import ProductCondition, ProductCategory, User if they are needed by the server

namespace server.Models // Adjusted namespace to server.Models
{
    public abstract class Product
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ProductCondition Condition { get; set; }
        public ProductCategory Category { get; set; }
        public List<ProductTag> Tags { get; set; }
        public List<Image> Images { get; set; }
        public User Seller { get; set; }
    }
} 