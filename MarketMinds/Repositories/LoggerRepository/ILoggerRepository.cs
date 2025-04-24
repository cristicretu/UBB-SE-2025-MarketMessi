namespace Marketplace_SE.Repositories
{
    public interface ILoggerRepository
    {
        void LogInfo(string message);
        void LogError(string message);
    }
}
