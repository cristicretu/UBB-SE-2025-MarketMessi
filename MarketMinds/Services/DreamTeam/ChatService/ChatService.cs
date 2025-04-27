using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds.Repositories.ChatRepository;
using NUnit.Framework.Interfaces;

namespace MarketMinds.Services.DreamTeam.ChatService
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository chatRepository;

        private User currentUser;
        private User targetUser;
        private Conversation? currentConversation;
        private List<Message> initialMessages = new List<Message>();
        private long lastMessageTimestamp = 0;

        public ChatService(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        public void Initialize(User currentUser, User targetUser)
        {
            this.currentUser = currentUser;
            this.targetUser = targetUser;

            try
            {
                currentConversation = chatRepository.GetConversation(currentUser.Id, targetUser.Id);

                if (currentConversation == null)
                {
                    currentConversation = chatRepository.CreateConversation(currentUser.Id, targetUser.Id);
                }
                if (currentConversation == null)
                {
                    throw new Exception("Failed to create or retrieve conversation");
                }

                initialMessages = chatRepository.GetMessages(currentConversation.Id);
                lastMessageTimestamp = initialMessages
                    .Where(message => message.Creator != currentUser.Id)
                    .Select(message => message.Timestamp)
                    .DefaultIfEmpty(0)
                    .Max();
            }
            catch (Exception exception)
            {
                throw new Exception("Error initializing chat service: " + exception.Message);
            }
        }

        public bool SendTextMessage(string textMessage)
        {
            if (currentConversation == null || currentUser == null || string.IsNullOrEmpty(textMessage))
            {
                return false;
            }

            var message = new Message
            {
                ConversationId = currentConversation.Id,
                Creator = currentUser.Id,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ContentType = "text",
                Content = textMessage
            };

            try
            {
                bool success = chatRepository.AddMessage(message);
                if (success)
                {
                    initialMessages.Add(message);
                    lastMessageTimestamp = message.Timestamp;
                }
                return success;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public bool SendImageMessage(byte[] imageData)
        {
            if (currentConversation == null || currentUser == null || imageData.Length == 0)
            {
                return false;
            }

            var message = new Message
            {
                ConversationId = currentConversation.Id,
                Creator = currentUser.Id,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ContentType = "image",
                Content = Convert.ToBase64String(imageData)
            };

            try
            {
                bool success = chatRepository.AddMessage(message);
                if (success)
                {
                    initialMessages.Add(message);
                    lastMessageTimestamp = message.Timestamp;
                }
                return success;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public List<Message> GetInitialMessages()
        {
           return new List<Message>(initialMessages);
        }

        public List<Message> CheckForNewMessages()
        {
            if (currentConversation == null)
            {
                return new List<Message>();
            }
            try
            {
                var newMessages = chatRepository.GetMessages(currentConversation.Id, lastMessageTimestamp);

                long newMaxMessageTimestamp = newMessages
                    .Where(message => message.Creator != currentUser.Id)
                    .Select(message => message.Timestamp)
                    .DefaultIfEmpty(lastMessageTimestamp)
                    .Max();

                if (newMaxMessageTimestamp > lastMessageTimestamp)
                {
                    lastMessageTimestamp = newMaxMessageTimestamp;
                    initialMessages.AddRange(newMessages);
                    return newMessages;
                }
                else
                {
                    return new List<Message>();
                }
            }
            catch (Exception exception)
            {
                return new List<Message>();
            }
        }
        public Conversation? GetConversation()
        {
            return currentConversation;
        }
    }
}