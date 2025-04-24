using System;
using System.Collections.Generic;

namespace server.Models
{
    public class BorrowProduct : Product
    {
        public DateTime TimeLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public float DailyRate { get; set; }
        public bool IsBorrowed { get; set; }
        
        // Navigation properties
        public ICollection<BorrowProductImage> Images { get; set; }
        public ICollection<BorrowProductProductTag> ProductTags { get; set; }
        
        public BorrowProduct()
        {
            Images = new List<BorrowProductImage>();
            ProductTags = new List<BorrowProductProductTag>();
        }
        
        public BorrowProduct(int id, string title, string description, User seller,
            Condition condition, Category category, DateTime timeLimit, 
            float dailyRate)
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            Condition = condition;
            Category = category;
            TimeLimit = timeLimit;
            DailyRate = dailyRate;
            IsBorrowed = false;
            Images = new List<BorrowProductImage>();
            ProductTags = new List<BorrowProductProductTag>();
        }
    }
} 
