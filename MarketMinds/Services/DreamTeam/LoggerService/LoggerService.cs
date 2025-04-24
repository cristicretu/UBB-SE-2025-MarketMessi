using Marketplace_SE.Repositories;

namespace Marketplace_SE.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly ILoggerRepository loggerRepository;

        public LoggerService(ILoggerRepository loggerRepository)
        {
            this.loggerRepository = loggerRepository;
        }

        public void LogInfo(string message)
        {
            loggerRepository.LogInfo(message);
        }

        public void LogError(string message)
        {
            loggerRepository.LogError(message);
        }
    }
}
