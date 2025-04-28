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

namespace MarketMinds.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl = "http://localhost:5000/api/message";

        public MessageRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            Debug.WriteLine($"MessageRepository created with baseUrl: {baseUrl}");
        }

        public async Task<Message> SendMessageAsync(Message message)
        {
            try
            {
                Debug.WriteLine($"Sending message to conversation ID: {message.ConversationId}");
                var jsonContent = JsonConvert.SerializeObject(message);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(baseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var sentMessage = await response.Content.ReadFromJsonAsync<Message>();
                    Debug.WriteLine($"Sent message with ID: {sentMessage.Id}");
                    return sentMessage;
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

        public async Task<Message> CreateMessageAsync(Message message)
        {
            try
            {
                Debug.WriteLine($"Creating message for conversation ID: {message.ConversationId}");
                var jsonContent = JsonConvert.SerializeObject(message);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(baseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var createdMessage = await response.Content.ReadFromJsonAsync<Message>();
                    Debug.WriteLine($"Created message with ID: {createdMessage.Id}");
                    return createdMessage;
                }
                else
                {
                    Debug.WriteLine($"Error creating message: {response.StatusCode}");
                    throw new Exception($"Failed to create message: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in CreateMessageAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            try
            {
                Debug.WriteLine($"Getting messages for conversation ID: {conversationId}");
                var response = await httpClient.GetAsync($"{baseUrl}/conversation/{conversationId}");

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
                Debug.WriteLine($"Exception in GetMessagesByConversationIdAsync: {ex.Message}");
                throw;
            }
        }
    }
}