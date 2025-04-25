using server.Models;
using System.Collections.Generic;

namespace MarketMinds.Repositories.HardwareSurveyRepository
{
    public interface IHardwareSurveyRepository
    {
        void SaveHardwareData(HardwareSurvey hardwareData);
        List<HardwareSurvey> GetAllHardwareSurveys();
    }
}
