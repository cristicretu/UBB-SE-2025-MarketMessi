using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.MessageService
{
    public class MessageService : IMessageService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl = "http://localhost:5000/api/message";

        public MessageService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Message> CreateMessageAsync(int conversationId, int userId, string content)
        {
            var createDto = new
            {
                ConversationId = conversationId,
                UserId = userId,
                Content = content
            };

            var response = await httpClient.PostAsJsonAsync(baseUrl, createDto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Message>();
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            var response = await httpClient.GetAsync($"{baseUrl}/conversation/{conversationId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Message>>();
        }
    }
}
