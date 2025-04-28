using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.DreamTeam.ChatService
{
    public class ChatService : IChatService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl = "http://localhost:5000/api";

        public ChatService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Conversation> CreateConversationAsync(int userId)
        {
            var requestData = new { UserId = userId };
            var response = await httpClient.PostAsJsonAsync($"{baseUrl}/conversation", requestData);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Conversation>();
        }

        public async Task<Conversation> GetConversationAsync(int conversationId)
        {
            var response = await httpClient.GetAsync($"{baseUrl}/conversation/{conversationId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Conversation>();
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            var response = await httpClient.GetAsync($"{baseUrl}/conversation/user/{userId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Conversation>>();
        }

        public async Task<Message> SendMessageAsync(int conversationId, int userId, string content)
        {
            var requestData = new
            {
                ConversationId = conversationId,
                UserId = userId,
                Content = content
            };

            var response = await httpClient.PostAsJsonAsync($"{baseUrl}/message", requestData);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Message>();
        }

        public async Task<List<Message>> GetMessagesAsync(int conversationId)
        {
            var response = await httpClient.GetAsync($"{baseUrl}/message/conversation/{conversationId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Message>>();
        }
    }
}