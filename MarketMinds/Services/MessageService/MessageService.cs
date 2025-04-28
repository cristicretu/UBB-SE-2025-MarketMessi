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

        public async Task<DomainLayer.Domain.Message> CreateMessageAsync(int conversationId, int userId, string content)
        {
            // Create a message object to pass to the repository
            var messageModel = new MarketMinds.Shared.Models.Message
            {
                ConversationId = conversationId,
                UserId = userId,
                Content = content
                // No ContentType or Timestamp in the shared model
            };

            var result = await messageRepository.CreateMessageAsync(messageModel);
            return ConvertToDomainModel(result);
        }

        public async Task<List<DomainLayer.Domain.Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            var messages = await messageRepository.GetMessagesByConversationIdAsync(conversationId);

            var domainMessages = new List<DomainLayer.Domain.Message>();
            foreach (var msg in messages)
            {
                domainMessages.Add(ConvertToDomainModel(msg));
            }

            return domainMessages;
        }

        // Helper method to convert between model types
        private DomainLayer.Domain.Message ConvertToDomainModel(MarketMinds.Shared.Models.Message sharedModel)
        {
            if (sharedModel == null)
            {
                return null;
            }
            return new DomainLayer.Domain.Message
            {
                Id = sharedModel.Id,
                ConversationId = sharedModel.ConversationId,
                UserId = sharedModel.UserId,
                Content = sharedModel.Content
                // No ContentType or Timestamp to map
            };
        }
    }
}
