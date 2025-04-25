using System.Collections.Generic;
using server.Models;

namespace MarketMinds.Repositories.LoggerRepository
{
    public interface ILoggerRepository
    {
        void LogInfo(string message);
        void LogError(string message);
        void LogWarning(string message);
        List<Log> GetRecentLogs(int count = 100);
        List<Log> GetLogsByLevel(string logLevel, int count = 100);
    }
}
