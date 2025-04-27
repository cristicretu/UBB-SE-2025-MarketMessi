using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.ConversationService
{
    public interface IConversationService
    {
        Task<Conversation> CreateConversationAsync(int userId);
        Task<Conversation> GetConversationByIdAsync(int conversationId);
        Task<List<Conversation>> GetUserConversationsAsync(int userId);
    }
}
