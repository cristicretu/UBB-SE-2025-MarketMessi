using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("HardwareSurvey")]
    public class HardwareSurvey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeviceID { get; set; }

        [Required]
        [MaxLength(50)]
        public string DeviceType { get; set; }

        [Required]
        [MaxLength(50)]
        public string OperatingSystem { get; set; }

        [Required]
        [MaxLength(50)]
        public string OSVersion { get; set; }

        [MaxLength(50)]
        public string BrowserName { get; set; }

        [MaxLength(50)]
        public string BrowserVersion { get; set; }

        [Required]
        [MaxLength(50)]
        public string ScreenResolution { get; set; }

        [Required]
        [MaxLength(50)]
        public string AvailableRAM { get; set; }

        [MaxLength(100)]
        public string CPUInformation { get; set; }

        [MaxLength(100)]
        public string GPUInformation { get; set; }

        [Required]
        [MaxLength(50)]
        public string ConnectionType { get; set; }

        [Column("Timestamp")]
        public DateTime SurveyTimestamp { get; set; }

        [Required]
        [MaxLength(50)]
        public string AppVersion { get; set; }
    }
} 