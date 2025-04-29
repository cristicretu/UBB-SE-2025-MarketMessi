using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.DreamTeam.ChatbotService
{
    public interface IChatbotService
    {
        Node InitializeChat();
        bool SelectOption(Node selectedNode);
        string GetCurrentResponse();
        IEnumerable<Node> GetCurrentOptions();
        bool IsInteractionActive();
        Task<string> GetBotResponseAsync(string userMessage, bool isWelcomeMessage = false);
        void SetCurrentUser(MarketMinds.Shared.Models.User user);
    }
}