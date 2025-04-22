using System;
using System.Collections.Generic;

namespace server.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ReviewerId { get; set; }
        public int SellerId { get; set; }
        public string Description { get; set; }
        public float Rating { get; set; }
        
        // Navigation properties
        public User Reviewer { get; set; }
        public User Seller { get; set; }
        public ICollection<ReviewPicture> Pictures { get; set; }
        
        public Review() 
        {
            Pictures = new List<ReviewPicture>();
        }
        
        public Review(int id, int reviewerId, int sellerId, string description, float rating)
        {
            Id = id;
            ReviewerId = reviewerId;
            SellerId = sellerId;
            Description = description;
            Rating = rating;
            Pictures = new List<ReviewPicture>();
        }
    }
} 