using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using DomainLayer.Domain;
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

        public ChatbotService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            isActive = false;
            Debug.WriteLine($"ChatbotService created with baseUrl: {baseUrl}");
            // Create default node
            currentNode = new Node
            {
                Id = 1,
                Response = "Welcome to the chat bot. How can I help you?",
                ButtonLabel = "Start Chat",
                LabelText = "Welcome",
                Children = new List<Node>()
            };
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

                // Create an error node
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
            // Always set the current node to the selected node
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

            Debug.WriteLine($"GetBotResponseAsync called at {DateTime.Now:HH:mm:ss.fff}. Message: '{userMessage.Substring(0, Math.Min(30, userMessage.Length))}...'");
            Debug.WriteLine($"Is welcome message: {isWelcomeMessage}");
            try
            {
                if (isWelcomeMessage)
                {
                    Debug.WriteLine("Returning welcome message instead of API call");
                    stopwatch.Stop();
                    Debug.WriteLine($"GetBotResponseAsync completed in {stopwatch.ElapsedMilliseconds}ms");
                    return "Hello! I'm your shopping assistant. How can I help you today?";
                }

                string apiKey = await GetApiKeyAsync();
                if (string.IsNullOrEmpty(apiKey))
                {
                    Debug.WriteLine("API key is null or empty");
                    return "I'm sorry, I'm having trouble connecting to my brain right now. Please try again later.";
                }

                Debug.WriteLine($"API key loaded successfully (length: {apiKey.Length})");

                // The server-side controller expects a simple request with a Message field
                // Not the complex Gemini API format
                var requestData = new
                {
                    Message = userMessage
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                Debug.WriteLine($"Request JSON: {jsonContent}");

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Remove the API key from the URL as it's handled on the server
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

        private async Task<string> GetApiKeyAsync()
        {
            try
            {
                Debug.WriteLine("Getting API key from local settings...");

                // Use hardcoded API key for testing if other methods fail
                // This is a temporary fix to prevent the app from crashing
                const string fallbackApiKey = "AIzaSyDdvtwFYdF_Aqy8UpH0zrfQEzoCpg_KoPc";

                try
                {
                    // Make sure we don't access ApplicationData.Current properties when not on UI thread
                    // or when the application isn't fully initialized
                    if (ApplicationData.Current != null)
                    {
                        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                        if (localSettings != null && localSettings.Values.TryGetValue("GeminiApiKey", out object value))
                        {
                            string apiKey = value.ToString();
                            Debug.WriteLine($"API key found in settings (length: {apiKey.Length})");
                            return apiKey;
                        }

                        Debug.WriteLine("API key not found in local settings, trying to load from file");

                        try
                        {
                            StorageFolder appFolder = ApplicationData.Current.LocalFolder;
                            if (appFolder != null)
                            {
                                try
                                {
                                    StorageFile apiKeyFile = await appFolder.GetFileAsync("gemini_api_key.txt");
                                    string apiKey = await FileIO.ReadTextAsync(apiKeyFile);

                                    if (!string.IsNullOrWhiteSpace(apiKey))
                                    {
                                        apiKey = apiKey.Trim();
                                        Debug.WriteLine($"API key loaded from file (length: {apiKey.Length})");

                                        // Save to settings for next time
                                        localSettings.Values["GeminiApiKey"] = apiKey;

                                        return apiKey;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("API key file exists but is empty");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error loading API key from file: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error accessing application folder: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in ApplicationData access: {ex.Message}");
                }

                // Return fallback API key if we couldn't get one from settings or file
                Debug.WriteLine("Using fallback API key");
                return fallbackApiKey;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetApiKeyAsync: {ex.Message}");
                // Use the hardcoded fallback API key as a last resort
                Debug.WriteLine("Using fallback API key after exception");
                return "AIzaSyDdvtwFYdF_Aqy8UpH0zrfQEzoCpg_KoPc";
            }
        }
    }

    public class ChatbotResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}