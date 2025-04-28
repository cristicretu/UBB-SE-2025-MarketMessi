using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using MarketMinds.Shared.Models;
using Newtonsoft.Json;
using Windows.Storage;

namespace MarketMinds.Services.DreamTeam.ChatbotService
{
    public class ChatbotService : IChatbotService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl = "http://localhost:5000/api/chatbot";
        private Node currentNode;
        private bool isActive;
        private static User currentUser;

        public ChatbotService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            isActive = false;
            Debug.WriteLine($"ChatbotService created with baseUrl: {baseUrl}");
            currentNode = new Node
            {
                Id = 1,
                Response = "Welcome to the chat bot. How can I help you?",
                ButtonLabel = "Start Chat",
                LabelText = "Welcome",
                Children = new List<Node>()
            };
        }

        public void SetCurrentUser(User user)
        {
            if (user == null)
            {
                Debug.WriteLine("[SERVICE] WARNING: Attempted to set null user");
                return;
            }
            currentUser = user;
        }

        public Node InitializeChat()
        {
            try
            {
                isActive = true;
                return currentNode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing chat: {ex.Message}");
                isActive = false;

                currentNode = new Node
                {
                    Id = -1,
                    Response = "Error: Unable to initialize the chat. Please try again later.",
                    ButtonLabel = "Restart",
                    LabelText = "Chat Initialization Error",
                    Children = new List<Node>()
                };

                return currentNode;
            }
        }

        public bool IsInteractionActive()
        {
            return isActive && currentNode != null;
        }

        public bool SelectOption(Node selectedNode)
        {
            currentNode = selectedNode;
            isActive = currentNode != null;
            return true;
        }

        public IEnumerable<Node> GetCurrentOptions()
        {
            if (currentNode == null || currentNode.Children == null)
            {
                return new List<Node>();
            }

            return currentNode.Children;
        }

        public string GetCurrentResponse()
        {
            return currentNode?.Response ?? "Chat not initialized. Please try again.";
        }

        public async Task<string> GetBotResponseAsync(string userMessage, bool isWelcomeMessage = false)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                if (isWelcomeMessage)
                {
                    Debug.WriteLine("Returning welcome message instead of API call");
                    stopwatch.Stop();
                    Debug.WriteLine($"GetBotResponseAsync completed in {stopwatch.ElapsedMilliseconds}ms");
                    return "Hello! I'm your shopping assistant. How can I help you today?";
                }

                int? userId = currentUser?.Id;
                var requestData = new
                {
                    Message = userMessage,
                    UserId = userId
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                Debug.WriteLine($"Request JSON: {jsonContent}");

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = baseUrl;
                Debug.WriteLine($"Preparing HTTP request to URL: {url}");

                Debug.WriteLine("Sending HTTP request...");
                var response = await httpClient.PostAsync(url, content);

                Debug.WriteLine($"HTTP response received: Status {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response JSON: {resultJson.Substring(0, Math.Min(100, resultJson.Length))}...");

                    try
                    {
                        // Parse the response from our server - it's now a simple structure
                        var responseObject = JsonConvert.DeserializeObject<ChatbotResponse>(resultJson);
                        string responseText = responseObject.Message;

                        Debug.WriteLine($"Successfully extracted response text: '{responseText.Substring(0, Math.Min(30, responseText.Length))}...'");
                        stopwatch.Stop();
                        Debug.WriteLine($"GetBotResponseAsync completed in {stopwatch.ElapsedMilliseconds}ms");

                        return responseText;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error extracting text from response: {ex.Message}");
                        Debug.WriteLine($"Response JSON: {resultJson}");
                        return "I'm sorry, I couldn't understand my own thoughts. Please try again.";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error response from API: {errorContent}");
                    return $"I'm sorry, I'm having trouble thinking right now (Error: {response.StatusCode}).";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetBotResponseAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return "I'm sorry, an error occurred while processing your request. Please try again later.";
            }
            finally
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                    Debug.WriteLine($"GetBotResponseAsync completed with error in {stopwatch.ElapsedMilliseconds}ms");
                }
            }
        }
    }

    public class ChatbotResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}