using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Domain
{
    public class BuyProduct : Product
    {
        public double Price { get; set; }

        public BuyProduct() : base()
        {
            Price = 0;
            Tags = new List<ProductTag>();
            Images = new List<Image>();
        }

        public BuyProduct(int id, string title, string description, User seller, ProductCondition productCondition, ProductCategory productCategory,
            List<ProductTag> productTags, List<Image> images, double price)
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
    }
}
