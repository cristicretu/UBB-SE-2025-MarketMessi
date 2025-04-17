using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        // Navigation properties
        [ForeignKey("ParentId")]
        public Category Parent { get; set; }
        public ICollection<Category> Subcategories { get; set; }
        public ICollection<AuctionProduct> Products { get; set; }

        public Category()
        {
            Subcategories = new List<Category>();
            Products = new List<AuctionProduct>();
        }

        public Category(string name, int? parentId = null)
        {
            Name = name;
            ParentId = parentId;
            Subcategories = new List<Category>();
            Products = new List<AuctionProduct>();
        }
    }
} 