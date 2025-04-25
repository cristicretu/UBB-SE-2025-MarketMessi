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
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Default constructor for JSON deserialization
        public ProductCondition()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
        }

        public ProductCondition(int id, string name, string description)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        // Keep DisplayTitle property for backward compatibility or UI binding if needed
        // You might need to adjust UI bindings if they used DisplayTitle directly
        [System.Text.Json.Serialization.JsonIgnore] // Don't try to serialize/deserialize this again
        public string DisplayTitle => Name;
    }
}
