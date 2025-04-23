using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.Models
{
    public class BuyProduct : Product
    {
        public float Price { get; set; }
        
        // Navigation properties
        public ICollection<BuyProductImage> Images { get; set; }
        public ICollection<BuyProductProductTag> ProductTags { get; set; }

        public BuyProduct()
        {
            Images = new List<BuyProductImage>();
            ProductTags = new List<BuyProductProductTag>();
        }

        public BuyProduct(int id, string title, string description, User seller, 
            Condition condition, Category category, float price)
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            Condition = condition;
            Category = category;
            Price = price;
            Images = new List<BuyProductImage>();
            ProductTags = new List<BuyProductProductTag>();
        }
    }
} 