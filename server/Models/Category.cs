using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("ProductCategories")]
    public class Category
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        // Navigation properties
        public ICollection<AuctionProduct> Products { get; set; }

        public Category()
        {
            Products = new List<AuctionProduct>();
        }

        public Category(string name, string description = null)
        {
            Name = name;
            Description = description;
            Products = new List<AuctionProduct>();
        }
    }
} 