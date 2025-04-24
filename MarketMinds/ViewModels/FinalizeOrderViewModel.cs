using System;
using System.Threading.Tasks;
using Marketplace_SE.Rating;
using Marketplace_SE.Services;

namespace Marketplace_SE.ViewModels
{
    public class FinalizeOrderViewModel
    {
        private readonly IRatingService ratingService;
        private readonly IHardwareSurveyService hardwareSurveyService;
        private readonly ILoggerService loggerService;

        public FinalizeOrderViewModel(IRatingService ratingService, IHardwareSurveyService hardwareSurveyService, ILoggerService loggerService)
        {
            this.ratingService = ratingService;
            this.hardwareSurveyService = hardwareSurveyService;
            this.loggerService = loggerService;
        }

        public void FinalizeOrder()
        {
            try
            {
                // Simulate rating and hardware survey logic
                loggerService.LogInfo("Starting order finalization...");

                // Simulate rating logic
                loggerService.LogInfo("Showing rating dialog...");
                System.Threading.Thread.Sleep(500); // Simulate synchronous operation
                loggerService.LogInfo("Rating dialog completed.");

                // Simulate hardware survey logic
                loggerService.LogInfo("Showing hardware survey dialog...");
                System.Threading.Thread.Sleep(500); // Simulate synchronous operation
                loggerService.LogInfo("Hardware survey dialog completed.");

                loggerService.LogInfo("Order finalized successfully.");
            }
            catch (Exception ex)
            {
                loggerService.LogError($"Error finalizing order: {ex.Message}");
                throw;
            }
        }
    }
}
