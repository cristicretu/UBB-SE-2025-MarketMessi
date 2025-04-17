using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("ProductConditions")]
    public class ProductCondition
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("title")]
        public string Title { get; set; } = string.Empty;
        
        // DisplayTitle property for backward compatibility
        [NotMapped]
        public string DisplayTitle
        {
            get { return Title; }
            set { Title = value; }
        }
        
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        // Constructor used by Repository
        public ProductCondition(int id, string displayTitle, string description)
        {
            this.Id = id;
            this.Title = displayTitle;
            this.Description = description;
        }

        // Default constructor for framework needs
        public ProductCondition() { }
    }
} 