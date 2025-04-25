using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Logs")]
    public class Log
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string LogLevel { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        // Parameterless constructor for EF
        public Log() { }
        
        // Constructor for convenience
        public Log(string logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
            Timestamp = DateTime.UtcNow;
        }
    }
} 