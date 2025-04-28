using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    [Table("ProductConditions")]
    public class Condition
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        // Navigation property
        public ICollection<AuctionProduct> Products { get; set; }

        public Condition()
        {
            Products = new List<AuctionProduct>();
        }

        public Condition(string name)
        {
            Name = name;
            Products = new List<AuctionProduct>();
        }

        public Condition(string name, string description)
        {
            Name = name;
            Description = description;
            Products = new List<AuctionProduct>();
        }
    }
}