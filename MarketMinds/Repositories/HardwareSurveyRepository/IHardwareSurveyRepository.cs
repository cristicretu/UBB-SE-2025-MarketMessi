using Marketplace_SE.HardwareSurvey;

namespace Marketplace_SE.Repositories
{
    public interface IHardwareSurveyRepository
    {
        void SaveHardwareData(HardwareData hardwareData); // Synchronous method for saving hardware data
    }
}
