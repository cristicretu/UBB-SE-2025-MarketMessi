using Marketplace_SE.HardwareSurvey;

namespace Marketplace_SE.Services
{
    public interface IHardwareSurveyService
    {
        void SaveHardwareData(HardwareData hardwareData); // Synchronous method for saving hardware data
    }
}
