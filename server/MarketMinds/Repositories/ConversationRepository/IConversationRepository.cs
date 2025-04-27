using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using server.Models;

namespace MarketMinds.Repositories.ConversationRepository
{
    public interface IConversationRepository
    {
        Task<Conversation> CreateConversationAsync(Conversation conversation);
        Task<Conversation> GetConversationByIdAsync(int conversationId);
        Task<List<Conversation>> GetConversationsByUserIdAsync(int userId);
    }
}
