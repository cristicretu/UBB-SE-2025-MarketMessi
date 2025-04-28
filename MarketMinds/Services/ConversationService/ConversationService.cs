using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository.ConversationRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.ConversationService
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository conversationRepository;

        public ConversationService(IConversationRepository conversationRepository)
        {
            this.conversationRepository = conversationRepository;
        }

        public async Task<DomainLayer.Domain.Conversation> CreateConversationAsync(int userId)
        {
            var conversationModel = new MarketMinds.Shared.Models.Conversation
            {
                UserId = userId
            };

            var result = await conversationRepository.CreateConversationAsync(conversationModel);

            // Convert from shared model to domain model
            return ConvertToDomainModel(result);
        }

        public async Task<DomainLayer.Domain.Conversation> GetConversationByIdAsync(int conversationId)
        {
            var result = await conversationRepository.GetConversationByIdAsync(conversationId);

            // Convert from shared model to domain model
            return ConvertToDomainModel(result);
        }

        public async Task<List<DomainLayer.Domain.Conversation>> GetUserConversationsAsync(int userId)
        {
            var results = await conversationRepository.GetConversationsByUserIdAsync(userId);

            // Convert list from shared model to domain model
            var domainConversations = new List<DomainLayer.Domain.Conversation>();
            foreach (var conv in results)
            {
                domainConversations.Add(ConvertToDomainModel(conv));
            }

            return domainConversations;
        }

        // Helper method to convert between model types
        private DomainLayer.Domain.Conversation ConvertToDomainModel(MarketMinds.Shared.Models.Conversation sharedModel)
        {
            if (sharedModel == null)
            {
                return null;
            }
            return new DomainLayer.Domain.Conversation
            {
                Id = sharedModel.Id,
                UserId = sharedModel.UserId
                // Map other properties as needed
            };
        }
    }
}
