using System.Threading.Tasks;

namespace MarketMinds.Services.ChatbotService
{
    public interface IChatbotService
    {
        Task<string> GetBotResponseAsync(string userMessage);
    }
}