using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Domain
{
    public class BorrowProduct : Product
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime TimeLimit { get; set; }
        public float DailyRate { get; set; }
        public bool IsBorrowed { get; set; }

        // Parameterless constructor for JSON deserialization
        public BorrowProduct() { }

        public BorrowProduct(int id, string title, string description, User seller, ProductCondition productCondition, ProductCategory productCategory,
            List<ProductTag> productTags, List<Image> images, DateTime timeLimit, DateTime startDate, DateTime endDate, float dailyRate, bool isBorrowed)
        {
            this.Id = id;
            this.Description = description;
            this.Title = title;
            this.Seller = seller;
            this.Condition = productCondition;
            this.Category = productCategory;
            this.Tags = productTags;
            this.Images = images;
            this.TimeLimit = timeLimit;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.DailyRate = dailyRate;
            this.IsBorrowed = isBorrowed;
        }

    }
}
