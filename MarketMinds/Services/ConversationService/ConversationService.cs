using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.ConversationService
{
    public class ConversationService : IConversationService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl = "http://localhost:5000/api/conversation";

        public ConversationService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Conversation> CreateConversationAsync(int userId)
        {
            var createDto = new { UserId = userId };

            var response = await httpClient.PostAsJsonAsync(baseUrl, createDto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Conversation>();
        }

        public async Task<Conversation> GetConversationByIdAsync(int conversationId)
        {
            var response = await httpClient.GetAsync($"{baseUrl}/{conversationId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Conversation>();
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            var response = await httpClient.GetAsync($"{baseUrl}/user/{userId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Conversation>>();
        }
    }
}
