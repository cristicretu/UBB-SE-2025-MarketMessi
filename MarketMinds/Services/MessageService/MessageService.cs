using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.MessageService
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository messageRepository;

        public MessageService(IMessageRepository messageRepository)
        {
            this.messageRepository = messageRepository;
        }

        public async Task<Message> CreateMessageAsync(int conversationId, int userId, string content)
        {
            // Create a message object to pass to the repository
            var messageModel = new Message
            {
                ConversationId = conversationId,
                UserId = userId,
                Content = content
            };

            var result = await messageRepository.CreateMessageAsync(messageModel);
            return result;
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            var messages = await messageRepository.GetMessagesByConversationIdAsync(conversationId);
            return messages;
        }
    }
}
