using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public MessageRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public List<Message> GetMessagesForConversation(int conversationId)
        {
            var response = httpClient.GetAsync($"Message/conversation/{conversationId}").Result;
            response.EnsureSuccessStatusCode();
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var messages = new List<Message>();
            var responseJsonArray = JsonNode.Parse(responseJson)?.AsArray();

            if (responseJsonArray != null)
            {
                foreach (var responseJsonItem in responseJsonArray)
                {
                    var id = responseJsonItem["id"]?.GetValue<int>() ?? 0;
                    var content = responseJsonItem["content"]?.GetValue<string>() ?? string.Empty;
                    var senderId = responseJsonItem["senderId"]?.GetValue<int>() ?? 0;
                    var timestamp = responseJsonItem["timestamp"]?.GetValue<DateTime>() ?? DateTime.MinValue;
                    messages.Add(new Message(id, content, senderId, conversationId, timestamp));
                }
            }
            return messages;
        }

        public Message SendMessage(int conversationId, string content)
        {
            var requestContent = new StringContent(
                $"{{\"conversationId\":{conversationId},\"content\":\"{content}\"}}",
                System.Text.Encoding.UTF8,
                "application/json");
            var response = httpClient.PostAsync("Message", requestContent).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var jsonObject = JsonNode.Parse(json);

            if (jsonObject == null)
            {
                throw new InvalidOperationException("Failed to parse the server response.");
            }

            var id = jsonObject["id"]?.GetValue<int>() ?? 0;
            var messageContent = jsonObject["content"]?.GetValue<string>() ?? string.Empty;
            var senderId = jsonObject["senderId"]?.GetValue<int>() ?? 0;
            var timestamp = jsonObject["timestamp"]?.GetValue<DateTime>() ?? DateTime.MinValue;
            return new Message(id, messageContent, senderId, conversationId, timestamp);
        }

        public void DeleteMessage(int messageId)
        {
            var response = httpClient.DeleteAsync($"Message/{messageId}").Result;
            response.EnsureSuccessStatusCode();
        }

        Task<Message> IMessageRepository.CreateMessageAsync(int conversationId, int userId, string content)
        {
            throw new NotImplementedException();
        }

        Task<List<Message>> IMessageRepository.GetMessagesByConversationIdAsync(int conversationId)
        {
            throw new NotImplementedException();
        }
    }
} 