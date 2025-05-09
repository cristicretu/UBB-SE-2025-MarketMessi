using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;


namespace MarketMinds.Shared.ProxyRepository
{
    public class ConversationProxyRepository : IConversationRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public ConversationProxyRepository(IConfiguration configuration)
        {
            this.httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            Debug.WriteLine($"ConversationRepository created with baseUrl: {apiBaseUrl}");
        }

        public async Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            try
            {
                Debug.WriteLine($"Creating conversation for user ID: {conversation.UserId}");
                var requestData = new
                {
                    UserId = conversation.UserId
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiBaseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var createdConversation = await response.Content.ReadFromJsonAsync<Conversation>();
                    Debug.WriteLine($"Created conversation with ID: {createdConversation.Id}");
                    return createdConversation;
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

        public async Task<Conversation> GetConversationByIdAsync(int conversationId)
        {
            try
            {
                Debug.WriteLine($"Getting conversation with ID: {conversationId}");
                var response = await httpClient.GetAsync($"{apiBaseUrl}/{conversationId}");

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
                Debug.WriteLine($"Exception in GetConversationByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Conversation>> GetConversationsByUserIdAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"Getting conversations for user ID: {userId}");
                var response = await httpClient.GetAsync($"{apiBaseUrl}/user/{userId}");

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
                Debug.WriteLine($"Exception in GetConversationsByUserIdAsync: {ex.Message}");
                throw;
            }
        }
    }
}