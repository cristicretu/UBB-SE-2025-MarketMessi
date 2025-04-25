namespace Marketplace_SE.Services
{
    public interface ILoggerService
    {
        void LogInfo(string message);
        void LogError(string message);
    }
}
