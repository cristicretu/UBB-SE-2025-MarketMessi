using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IMessageRepository
    {
        Task<Message> CreateMessageAsync(int conversationId, int userId, string content);

        Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId);
    }
}
