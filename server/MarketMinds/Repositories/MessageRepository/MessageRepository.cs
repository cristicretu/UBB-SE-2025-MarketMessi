using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MarketMinds.Shared.Models;
using Server.DataAccessLayer;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Repositories.MessageRepository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext context;

        public MessageRepository(ApplicationDbContext dbContext)
        {
            this.context = dbContext;
        }

        public async Task<Message> CreateMessageAsync(int conversationId, int userId, string content)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                UserId = userId,
                Content = content
            };
            
            await context.Messages.AddAsync(message);
            await context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            return await context.Messages
                .Where(m => m.ConversationId == conversationId)
                .ToListAsync();
        }
    }
}
