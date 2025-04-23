using System;

namespace server.Models
{
    public class ReviewPicture
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int ReviewId { get; set; }
        
        // Navigation property
        public Review Review { get; set; }
        
        public ReviewPicture() { }
        
        public ReviewPicture(int id, string url, int reviewId)
        {
            Id = id;
            Url = url;
            ReviewId = reviewId;
        }
    }
} 