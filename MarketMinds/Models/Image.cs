using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DomainLayer.Domain
{
    public class Image
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        // Default constructor for JSON deserialization
        public Image()
        {
            Url = string.Empty;
        }

        public Image(string url)
        {
            this.Url = url;
        }

        // Default constructor for JSON deserialization
        public Image()
        {
        }
    }
}
