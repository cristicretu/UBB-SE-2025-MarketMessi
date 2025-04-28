using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using DomainLayer.Domain;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Services.DreamTeam.ChatbotService
{
    public class ChatbotService : IChatbotService
    {
        private readonly IChatbotRepository chatbotRepository;
        private Node currentNode;
        private bool isActive;
        private static User currentUser;

        public ChatbotService(IChatbotRepository chatbotRepository)
        {
            this.chatbotRepository = chatbotRepository;
            isActive = false;
            Debug.WriteLine("ChatbotService created using IChatbotRepository");
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

                // Use the repository instead of making HTTP calls directly
                string response = await chatbotRepository.GetBotResponseAsync(userMessage, userId);

                stopwatch.Stop();
                Debug.WriteLine($"GetBotResponseAsync completed in {stopwatch.ElapsedMilliseconds}ms");
                return response;
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
}