using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Domain
{
    public class ProductCondition
    {
        public int Id { get; set; }
        public string DisplayTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Default constructor for JSON deserialization
        public ProductCondition()
        {
            Id = 0;
            DisplayTitle = string.Empty;
            Description = string.Empty;
        }

        public ProductCondition(int id, string displayTitle, string description)
        {
            this.Id = id;
            this.DisplayTitle = displayTitle;
            this.Description = description;
        }
    }
}
