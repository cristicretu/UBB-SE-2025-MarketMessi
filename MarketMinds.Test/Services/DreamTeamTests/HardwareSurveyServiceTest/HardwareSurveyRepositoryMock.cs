using System;
using Marketplace_SE.HardwareSurvey;
using Marketplace_SE.Repositories;

namespace MarketMinds.Test.Services.DreamTeamTests.HardwareSurveyServiceTest
{
    public class HardwareSurveyRepositoryMock : IHardwareSurveyRepository
    {
        private int saveCallCount = 0;

        public void SaveHardwareData(HardwareData hardwareData)
        {
            if (hardwareData == null)
            {
                throw new ArgumentNullException(nameof(hardwareData));
            }

            saveCallCount++;
        }

        public int GetSaveCallCount()
        {
            return saveCallCount;
        }
    }
}
