using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("username")]
        public string Username { get; set; }
        
        [Column("email")]
        public string Email { get; set; }
        
        [Column("password_hash")]
        public string PasswordHash { get; set; }
        
        [Column("full_name")]
        public string FullName { get; set; }
        
        [Column("address")]
        public string Address { get; set; }
        
        [Column("phone_number")]
        public string PhoneNumber { get; set; }
        
        [Column("userType")]
        public int UserType { get; set; }
        
        [Column("balance")]
        public float Balance { get; set; }
        
        [Column("rating")]
        public float Rating { get; set; }

        private const float MAX_BALANCE = 999999; // Consider if this constant is needed

        // Navigation properties
        public ICollection<AuctionProduct> SellingItems { get; set; }
        public ICollection<Bid> Bids { get; set; }

        public User()
        {
            SellingItems = new List<AuctionProduct>();
            Bids = new List<Bid>();
        }

        public User(string username, string email, string passwordHash)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            SellingItems = new List<AuctionProduct>();
            Bids = new List<Bid>();
        }
    }
} 