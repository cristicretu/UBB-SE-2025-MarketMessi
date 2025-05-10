using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MarketMinds.Shared.Models
{
    [Table("Users")]
    public class User : IdentityUser
    {
        // Identity properties are inherited:
        // Id (string), UserName, NormalizedUserName, Email, NormalizedEmail
        // PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, etc.

        // Keep legacy integer ID as a separate column for backwards compatibility
        [Column("legacy_id")]
        public int LegacyId { get; set; }

        // For backward compatibility with code that expects Id to be an int
        [NotMapped]
        public int IntId 
        { 
            get => LegacyId;
            set => LegacyId = value; 
        }

        // Override UserName for compatibility with existing Username property
        [NotMapped]
        public new string UserName
        {
            get => base.UserName ?? string.Empty;
            set => base.UserName = value;
        }

        // Keep existing Username property but map it to UserName
        [Column("username")]
        public string Username
        {
            get => UserName;
            set => UserName = value;
        }

        // Override PasswordHash to maintain compatibility
        [NotMapped]
        public new string PasswordHash
        {
            get => base.PasswordHash ?? string.Empty;
            set => base.PasswordHash = value;
        }

        [NotMapped]
        public string Password { get; set; } = string.Empty;

        [Column("userType")]
        public int UserType { get; set; }

        [Column("balance")]
        public double Balance { get; set; }

        [Column("rating")]
        public double Rating { get; set; }

        [NotMapped]
        public string Token { get; set; } = string.Empty;

        private const double MAX_BALANCE = 999999;

        // Navigation properties
        public ICollection<AuctionProduct> SellingItems { get; set; }
        public ICollection<Bid> Bids { get; set; }

        public User()
        {
            Token = string.Empty;
            Password = string.Empty;
            SellingItems = new List<AuctionProduct>();
            Bids = new List<Bid>();
        }

        public User(string username, string email, string passwordHash)
        {
            UserName = username;
            Email = email;
            PasswordHash = passwordHash;
            Token = string.Empty;
            Password = string.Empty;
            SellingItems = new List<AuctionProduct>();
            Bids = new List<Bid>();
        }

        public User(int legacyId, string username, string email, string token)
        {
            LegacyId = legacyId;
            UserName = username;
            Email = email;
            Token = token;
            Password = string.Empty;
            SellingItems = new List<AuctionProduct>();
            Bids = new List<Bid>();
        }
    }
}