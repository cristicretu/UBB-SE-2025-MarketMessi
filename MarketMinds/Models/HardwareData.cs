using System;

namespace Marketplace_SE.HardwareSurvey
{
    public class HardwareData
    {
        public int SurveyID { get; set; }
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
    }
} 