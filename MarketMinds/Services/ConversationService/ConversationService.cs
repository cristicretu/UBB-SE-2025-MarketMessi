using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
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

        public async Task<Conversation> CreateConversationAsync(int userId)
        {
            var conversationModel = new Conversation
            {
                UserId = userId
            };

            var result = await conversationRepository.CreateConversationAsync(conversationModel);

            // Return the result directly
            return result;
        }

        public async Task<Conversation> GetConversationByIdAsync(int conversationId)
        {
            var result = await conversationRepository.GetConversationByIdAsync(conversationId);
            return result;
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            var results = await conversationRepository.GetConversationsByUserIdAsync(userId);
            return results;
        }
    }
}
