using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;

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
    }
}
