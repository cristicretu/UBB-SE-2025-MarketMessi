using System;

namespace server.Models // Adjusted namespace to server.Models
{
    public class ProductTag
    {
        public int Id { get; set; }
        public string DisplayTitle { get; set; } = string.Empty;

        public ProductTag(int id, string displayTitle)
        {
            this.Id = id;
            this.DisplayTitle = displayTitle;
        }

        // Default constructor for potential framework needs (e.g., deserialization)
        public ProductTag() { }
    }
} 