using Marketplace_SE.HardwareSurvey;
using Marketplace_SE.Repositories;

namespace Marketplace_SE.Services
{
    public class HardwareSurveyService : IHardwareSurveyService
    {
        private readonly IHardwareSurveyRepository hardwareSurveyRepository;

        public HardwareSurveyService(IHardwareSurveyRepository hardwareSurveyRepository)
        {
            hardwareSurveyRepository = hardwareSurveyRepository;
        }

        public void SaveHardwareData(HardwareData hardwareData)
        {
            // Add any additional business logic here if needed
            hardwareSurveyRepository.SaveHardwareData(hardwareData);
        }
    }
}
