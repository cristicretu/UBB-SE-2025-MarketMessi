using System;

namespace server.Models
{
    // This class can be used for binding directly from JSON requests
    public class HardwareSurveyRequest
    {
        public string DeviceID { get; set; }
        public string DeviceType { get; set; }
        public string OperatingSystem { get; set; }
        public string OSVersion { get; set; }
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string ScreenResolution { get; set; }
        public string AvailableRAM { get; set; }
        public string CPUInformation { get; set; }
        public string GPUInformation { get; set; }
        public string ConnectionType { get; set; }
        public DateTime Timestamp { get; set; }
        public string AppVersion { get; set; }
        
        // Convert to the actual model
        public HardwareSurvey ToHardwareSurvey()
        {
            return new HardwareSurvey
            {
                DeviceID = this.DeviceID,
                DeviceType = this.DeviceType,
                OperatingSystem = this.OperatingSystem,
                OSVersion = this.OSVersion,
                BrowserName = this.BrowserName,
                BrowserVersion = this.BrowserVersion,
                ScreenResolution = this.ScreenResolution,
                AvailableRAM = this.AvailableRAM,
                CPUInformation = this.CPUInformation,
                GPUInformation = this.GPUInformation,
                ConnectionType = this.ConnectionType,
                SurveyTimestamp = this.Timestamp != default ? this.Timestamp : DateTime.UtcNow,
                AppVersion = this.AppVersion
            };
        }
    }
} 