using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
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

        [Column("passwordHash")]
        public string? PasswordHash { get; set; }

        [Column("userType")]
        public int UserType { get; set; }

        [Column("balance")]
        public double Balance { get; set; }

        [Column("rating")]
        public double Rating { get; set; }

        private const double MAX_BALANCE = 999999;

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