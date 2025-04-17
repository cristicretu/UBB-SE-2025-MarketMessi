using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Conditions")]
    public class Condition
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

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
    }
} 