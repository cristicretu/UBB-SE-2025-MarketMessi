using System;
using System.Collections.Generic;
using System.Linq;
using DomainLayer.Domain;
using MarketMinds.Repositories.ChatRepository;

namespace MarketMinds.Test.Services.ChatServiceTest
{
    public class ChatRepositoryMock : IChatRepository
    {
        private List<Conversation> _conversations;
        private List<Message> _messages;
        private int _getConversationCount;
        private int _createConversationCount;
        private int _getMessagesCount;
        private int _addMessageCount;

        public ChatRepositoryMock()
        {
            _conversations = new List<Conversation>();
            _messages = new List<Message>();
            _getConversationCount = 0;
            _createConversationCount = 0;
            _getMessagesCount = 0;
            _addMessageCount = 0;
        }

        public Conversation? GetConversation(int userId1, int userId2)
        {
            _getConversationCount++;
            return _conversations.FirstOrDefault(c =>
                (c.UserId1 == userId1 && c.UserId2 == userId2) ||
                (c.UserId1 == userId2 && c.UserId2 == userId1));
        }

        public Conversation CreateConversation(int userId1, int userId2)
        {
            _createConversationCount++;
            var newConversation = new Conversation
            {
                Id = _conversations.Count + 1,
                UserId1 = userId1,
                UserId2 = userId2
            };
            _conversations.Add(newConversation);
            return newConversation;
        }

        public List<Message> GetMessages(int conversationId, long sinceTimestamp = 0)
        {
            _getMessagesCount++;
            return _messages
                .Where(m => m.ConversationId == conversationId && m.Timestamp > sinceTimestamp)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        public bool AddMessage(Message message)
        {
            _addMessageCount++;
            _messages.Add(message);
            return true;
        }

        // Helper methods for testing
        public int GetConversationCount => _getConversationCount;
        public int CreateConversationCount => _createConversationCount;
        public int GetMessagesCount => _getMessagesCount;
        public int AddMessageCount => _addMessageCount;

        public void AddTestConversation(Conversation conversation)
        {
            _conversations.Add(conversation);
        }

        public void AddTestMessages(List<Message> messages)
        {
            _messages.AddRange(messages);
        }
    }
}