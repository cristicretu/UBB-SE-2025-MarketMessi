using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Newtonsoft.Json;

namespace MarketMinds.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly HttpClient httpClient;

        public MessageRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }

            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public async Task<Message> CreateMessageAsync(int conversationId, int userId, string content)
        {
            var createDto = new
            {
                ConversationId = conversationId,
                UserId = userId,
                Content = content
            };

            var response = await httpClient.PostAsJsonAsync("message", createDto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Message>();
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            var response = await httpClient.GetAsync($"message/conversation/{conversationId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Message>>();
        }
    }
}