using System;
using NUnit.Framework;
using Marketplace_SE.HardwareSurvey;
using Marketplace_SE.Services;
using MarketMinds.Test.Services.DreamTeamTests.HardwareSurveyServiceTest;

namespace MarketMinds.Test.Services.DreamTeamTests.HardwareSurveyServiceTest
{
    [TestFixture]
    public class HardwareSurveyServiceTest
    {
        private HardwareSurveyService hardwareSurveyService;
        private HardwareSurveyRepositoryMock hardwareSurveyRepositoryMock;
        private HardwareData sampleHardwareData;

        private const string VALID_DEVICE_ID = "device123";
        private const string VALID_DEVICE_TYPE = "Laptop";
        private const string VALID_OS = "Windows";
        private const string VALID_OS_VERSION = "10";
        private const string VALID_BROWSER_NAME = "Chrome";
        private const string VALID_BROWSER_VERSION = "90.0";
        private const string VALID_SCREEN_RESOLUTION = "1920x1080";
        private const string VALID_AVAILABLE_RAM = "16";
        private const string VALID_CPU = "Intel i7";
        private const string VALID_GPU = "NVIDIA GTX 1660";
        private const string VALID_CONNECTION = "WiFi";
        private const string VALID_APP_VERSION = "1.0.0";

        [SetUp]
        public void Setup()
        {
            hardwareSurveyRepositoryMock = new HardwareSurveyRepositoryMock();
            hardwareSurveyService = new HardwareSurveyService(hardwareSurveyRepositoryMock);

            sampleHardwareData = new HardwareData
            {
                DeviceID = VALID_DEVICE_ID,
                DeviceType = VALID_DEVICE_TYPE,
                OperatingSystem = VALID_OS,
                OSVersion = VALID_OS_VERSION,
                BrowserName = VALID_BROWSER_NAME,
                BrowserVersion = VALID_BROWSER_VERSION,
                ScreenResolution = VALID_SCREEN_RESOLUTION,
                AvailableRAM = VALID_AVAILABLE_RAM,
                CPUInformation = VALID_CPU,
                GPUInformation = VALID_GPU,
                ConnectionType = VALID_CONNECTION,
                Timestamp = DateTime.Now,
                AppVersion = VALID_APP_VERSION
            };
        }

        [Test]
        public void TestSaveHardwareData_ValidData_CallsRepository()
        {
            hardwareSurveyService.SaveHardwareData(sampleHardwareData);

            Assert.That(hardwareSurveyRepositoryMock.GetSaveCallCount(), Is.EqualTo(1));
        }

        [Test]
        public void TestSaveHardwareData_NullData_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => hardwareSurveyService.SaveHardwareData(null));
        }
    }
}
