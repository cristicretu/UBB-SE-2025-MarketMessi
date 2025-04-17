using System;

namespace server.Models // Adjusted namespace to server.Models
{
    public class Image
    {
        public string Url { get; set; } = string.Empty;

        public Image(string url)
        {
            this.Url = url;
        }

        // Default constructor for potential framework needs (e.g., deserialization)
        public Image() { }
    }
} 