using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using server.Models;

namespace server.Models
{
    public class BuyProduct : Product
    {
        public float Price { get; set; }
        public List<ProductTag> Tags { get; set; }
        public List<Image> Images { get; set; }

        public BuyProduct() : base()
        {
            Price = 0;
            Tags = new List<ProductTag>();
            Images = new List<Image>();
        }

        public BuyProduct(int id, string title, string description, User seller, Condition productCondition, Category productCategory,
            List<ProductTag> productTags, List<Image> images, float price)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.Seller = seller;
            this.Condition = productCondition;
            this.Category = productCategory;
            this.Tags = productTags;
            this.Images = images;
            this.Price = price;
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
            Images = new List<Image>();
            Tags = new List<ProductTag>();
        }
    }
} 
