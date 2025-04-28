using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Models;

namespace MarketMinds.Repositories.MessageRepository
{
    public interface IMessageRepository
    {
        Task<Message> CreateMessageAsync(Message message);
        Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId);
    }
}
