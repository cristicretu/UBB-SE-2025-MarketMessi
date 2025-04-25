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
        public double DailyRate { get; set; }
        public bool IsBorrowed { get; set; }

        // Add parameterless constructor for JSON deserialization
        public BorrowProduct()
        {
            // Required for JSON deserialization
        }
        public BorrowProduct(int id, string title, string description, User seller, ProductCondition productCondition, ProductCategory productCategory,
            List<ProductTag> productTags, List<Image> images, DateTime timeLimit, DateTime startDate, DateTime endDate, double dailyRate, bool isBorrowed)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.Seller = seller;
            this.Condition = productCondition;
            this.Category = productCategory;
            this.Tags = productTags;
            this.Images = images;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.TimeLimit = timeLimit;
            this.DailyRate = dailyRate;
            this.IsBorrowed = isBorrowed;
        }
    }
}
