using System;

namespace server.Models // Adjusted namespace
{
    public class ProductCondition
    {
        public int Id { get; set; }
        public string DisplayTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Constructor used by Repository
        public ProductCondition(int id, string displayTitle, string description)
        {
            this.Id = id;
            this.DisplayTitle = displayTitle;
            this.Description = description;
        }

        // Default constructor for framework needs
        public ProductCondition() { }
    }
} 