using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.MessageService
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _repository;

        public MessageService(IMessageRepository repository)
        {
            _repository = repository;
        }

        public List<Message> GetMessagesForConversation(int conversationId)
        {
            return _repository.GetMessagesForConversation(conversationId);
        }

        public Message SendMessage(int conversationId, string content)
        {
            return _repository.SendMessage(conversationId, content);
        }

        public void DeleteMessage(int messageId)
        {
            _repository.DeleteMessage(messageId);
        }

        Task<Message> IMessageService.CreateMessageAsync(int conversationId, int userId, string content)
        {
            throw new System.NotImplementedException();
        }

        Task<List<Message>> IMessageService.GetMessagesByConversationIdAsync(int conversationId)
        {
            throw new System.NotImplementedException();
        }
    }
}
