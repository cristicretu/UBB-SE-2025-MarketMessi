using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.MessageService
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository messageRepository;

        public MessageService(IMessageRepository repository)
        {
            messageRepository = repository;
        }

        public async Task<Message> CreateMessageAsync(int conversationId, int userId, string content)
        {
            return await messageRepository.CreateMessageAsync(conversationId, userId, content);
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            return await messageRepository.GetMessagesByConversationIdAsync(conversationId);
        }
    }
}
