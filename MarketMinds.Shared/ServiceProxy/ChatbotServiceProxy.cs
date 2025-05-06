using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Newtonsoft.Json;

namespace MarketMinds.ServiceProxy
{
    public class ChatbotServiceProxy : IChatbotRepository
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl = "http://localhost:5000/api/chatbot";

        public ChatbotServiceProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            Debug.WriteLine($"ChatbotRepository created with baseUrl: {baseUrl}");
        }

        public async Task<string> GetBotResponseAsync(string userMessage, int? userId = null)
        {
            try
            {
                Debug.WriteLine($"Getting bot response for message: {userMessage}");
                var requestData = new
                {
                    Message = userMessage,
                    UserId = userId
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(baseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<ChatbotResponse>(resultJson);
                    Debug.WriteLine($"Received bot response");
                    return responseObject.Message;
                }
                else
                {
                    Debug.WriteLine($"Error getting bot response: {response.StatusCode}");
                    throw new Exception($"Failed to get bot response: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetBotResponseAsync: {ex.Message}");
                throw;
            }
        }
    }

    public class ChatbotResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}