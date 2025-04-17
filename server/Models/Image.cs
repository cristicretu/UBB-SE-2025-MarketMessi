using System;

namespace server.Models
{
    /// <summary>
    /// Legacy Image class for backward compatibility with existing repository code
    /// </summary>
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