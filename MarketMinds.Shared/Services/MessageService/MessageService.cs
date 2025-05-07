using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.MessageService
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
            if (conversationId <= 0)
            {
                throw new ArgumentException("Conversation ID must be greater than zero.", nameof(conversationId));
            }
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content cannot be null or empty.", nameof(content));
            }

            var createDto = new CreateMessageDto
            {
                ConversationId = conversationId,
                UserId = userId,
                Content = content
            };

            return await messageRepository.CreateMessageAsync(createDto);
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            return await messageRepository.GetMessagesByConversationIdAsync(conversationId);
        }
    }
}
