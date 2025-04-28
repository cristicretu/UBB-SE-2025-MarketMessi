using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.MessageService
{
    public interface IMessageService
    {
        Task<Message> CreateMessageAsync(int conversationId, int userId, string content);
        Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId);
    }
}
