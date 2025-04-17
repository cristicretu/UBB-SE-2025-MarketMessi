using System;

namespace server.Models // Adjusted namespace
{
    public class Bid
    {
        public int Id { get; set; }
        public int BidderId { get; set; }
        public int ProductId { get; set; }
        public User Bidder { get; set; } // Assumes User is defined in server.Models
        public float Price { get; set; }
        public DateTime Timestamp { get; set; }

        // Constructor used by Repository (if needed, currently not seen)
        public Bid(User bidder, float price, DateTime timestamp)
        {
            Bidder = bidder;
            BidderId = bidder.Id;
            Price = price;
            Timestamp = timestamp;
        }

        // Default constructor for framework needs
        public Bid() { }
    }
} 