using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("BuyProducts")]
    public class BuyProduct : Product
    {
        [Column("price")]
        public double Price { get; set; }

        [NotMapped]
        public List<ProductTag> Tags { get; set; }

        [NotMapped]
        public List<Image> Images { get; set; }

        public BuyProduct() : base()
        {
            Price = 0;
            Tags = new List<ProductTag>();
            Images = new List<Image>();
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
            this.Images = images ?? new List<Image>();
            this.Price = price;
        }

        public BuyProduct(int id, string title, string description, User seller,
            Condition condition, Category category, double price)
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
