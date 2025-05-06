using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.ProxyRepository
{
    public class ChatProxyRepository : IChatRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;
        public ChatProxyRepository(IConfiguration configuration)
        {
            this.httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            Debug.WriteLine($"ChatRepository created with baseUrl: {apiBaseUrl}");
        }

        public async Task<Conversation> CreateConversationAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"Creating conversation for user ID: {userId}");
                var requestData = new { UserId = userId };
                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/conversation", content);

                if (response.IsSuccessStatusCode)
                {
                    var conversation = await response.Content.ReadFromJsonAsync<Conversation>();
                    Debug.WriteLine($"Created conversation with ID: {conversation.Id}");
                    return conversation;
                }
                else
                {
                    Debug.WriteLine($"Error creating conversation: {response.StatusCode}");
                    throw new Exception($"Failed to create conversation: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in CreateConversationAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Conversation> GetConversationAsync(int conversationId)
        {
            try
            {
                Debug.WriteLine($"Getting conversation with ID: {conversationId}");
                var response = await httpClient.GetAsync($"{apiBaseUrl}/conversation/{conversationId}");

                if (response.IsSuccessStatusCode)
                {
                    var conversation = await response.Content.ReadFromJsonAsync<Conversation>();
                    Debug.WriteLine($"Retrieved conversation with ID: {conversation.Id}");
                    return conversation;
                }
                else
                {
                    Debug.WriteLine($"Error getting conversation: {response.StatusCode}");
                    throw new Exception($"Failed to get conversation: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetConversationAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"Getting conversations for user ID: {userId}");
                var response = await httpClient.GetAsync($"{apiBaseUrl}/conversation/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var conversations = await response.Content.ReadFromJsonAsync<List<Conversation>>();
                    Debug.WriteLine($"Retrieved {conversations.Count} conversations");
                    return conversations;
                }
                else
                {
                    Debug.WriteLine($"Error getting conversations: {response.StatusCode}");
                    throw new Exception($"Failed to get conversations: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetUserConversationsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Message> SendMessageAsync(int conversationId, int userId, string content)
        {
            try
            {
                Debug.WriteLine($"Sending message to conversation ID: {conversationId}");
                var requestData = new
                {
                    ConversationId = conversationId,
                    UserId = userId,
                    Content = content
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/message", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadFromJsonAsync<Message>();
                    Debug.WriteLine($"Sent message with ID: {message.Id}");
                    return message;
                }
                else
                {
                    Debug.WriteLine($"Error sending message: {response.StatusCode}");
                    throw new Exception($"Failed to send message: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in SendMessageAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Message>> GetMessagesAsync(int conversationId)
        {
            try
            {
                Debug.WriteLine($"Getting messages for conversation ID: {conversationId}");
                var response = await httpClient.GetAsync($"{apiBaseUrl}/message/conversation/{conversationId}");

                if (response.IsSuccessStatusCode)
                {
                    var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
                    Debug.WriteLine($"Retrieved {messages.Count} messages");
                    return messages;
                }
                else
                {
                    Debug.WriteLine($"Error getting messages: {response.StatusCode}");
                    throw new Exception($"Failed to get messages: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetMessagesAsync: {ex.Message}");
                throw;
            }
        }
    }
}