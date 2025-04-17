using System;

namespace server.Models
{
    // This is a legacy class used by the repository
    public class Image
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;

        public Image(string url)
        {
            this.Url = url;
        }

        // Default constructor for potential framework needs
        public Image() { }
    }
} 