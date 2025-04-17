using System;

namespace server.Models // Adjusted namespace to server.Models
{
    public class ProductTag
    {
        public int Id { get; set; }
        public string DisplayTitle { get; set; } = string.Empty;
        // Adding Title property to match what's being referenced in ApplicationDbContext
        public string Title { 
            get { return DisplayTitle; }
            set { DisplayTitle = value; }
        }

        public ProductTag(int id, string displayTitle)
        {
            this.Id = id;
            this.DisplayTitle = displayTitle;
        }

        // Default constructor for potential framework needs (e.g., deserialization)
        public ProductTag() { }
    }
} 