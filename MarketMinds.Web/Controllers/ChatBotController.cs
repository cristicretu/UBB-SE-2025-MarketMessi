using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MarketMinds.Web.Models;
using MarketMinds.Shared.Services.DreamTeam.ChatService;
using MarketMinds.Shared.Services.DreamTeam.ChatbotService;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MarketMinds.Web.Controllers
{
    public class ChatBotController : Controller
    {
        private readonly ILogger<ChatBotController> _logger;
        private readonly IChatService _chatService;
        private readonly IChatbotService _chatbotService;

        public ChatBotController(
            ILogger<ChatBotController> logger,
            IChatService chatService,
            IChatbotService chatbotService)
        {
            _logger = logger;
            _chatService = chatService;
            _chatbotService = chatbotService;
        }

        [AllowAnonymous]
        public IActionResult Index(int? conversationId = null)
        {
            var viewModel = new ChatViewModel();
            
            try
            {
                // Get current user ID
                int userId = User.Identity.IsAuthenticated ? User.GetCurrentUserId() : 1;
                
                // Get conversation data from session
                string conversationsJson = HttpContext.Session.GetString("UserConversations");
                List<MarketMinds.Shared.Models.Conversation> userConversations;
                
                if (!string.IsNullOrEmpty(conversationsJson))
                {
                    userConversations = JsonConvert.DeserializeObject<List<MarketMinds.Shared.Models.Conversation>>(conversationsJson);
                }
                else
                {
                    // Create initial mock conversations
                    userConversations = new List<MarketMinds.Shared.Models.Conversation>();
                    for (int i = 1; i <= 3; i++)
                    {
                        userConversations.Add(new MarketMinds.Shared.Models.Conversation
                        {
                            Id = i,
                            UserId = userId
                        });
                    }
                    
                    // Save to session
                    HttpContext.Session.SetString("UserConversations", JsonConvert.SerializeObject(userConversations));
                }
                
                viewModel.Conversations = userConversations;
                
                // If a specific conversation is selected
                if (conversationId.HasValue && conversationId.Value > 0)
                {
                    // Find the selected conversation in our list
                    viewModel.CurrentConversation = viewModel.Conversations.FirstOrDefault(c => c.Id == conversationId.Value);
                    
                    // If it's not in our list but has a valid ID, create it
                    if (viewModel.CurrentConversation == null)
                    {
                        viewModel.CurrentConversation = new MarketMinds.Shared.Models.Conversation
                        {
                            Id = conversationId.Value,
                            UserId = userId
                        };
                        
                        userConversations.Add(viewModel.CurrentConversation);
                        HttpContext.Session.SetString("UserConversations", JsonConvert.SerializeObject(userConversations));
                    }
                    
                    // Get messages for this conversation
                    string messagesKey = $"Messages_{conversationId.Value}";
                    string messagesJson = HttpContext.Session.GetString(messagesKey);
                    
                    if (!string.IsNullOrEmpty(messagesJson))
                    {
                        var sessionMessages = JsonConvert.DeserializeObject<List<MessageViewModel>>(messagesJson);
                        
                        // Convert MessageViewModel to Message
                        viewModel.Messages = sessionMessages.Select(m => new MarketMinds.Shared.Models.Message
                        {
                            Id = m.Id,
                            ConversationId = m.ConversationId,
                            UserId = m.UserId,
                            Content = m.Content
                        }).ToList();
                    }
                    else
                    {
                        // Create empty message list
                        viewModel.Messages = new List<MarketMinds.Shared.Models.Message>();
                    }
                }
                // If user has conversations, select the first one by default
                else if (viewModel.Conversations.Count > 0)
                {
                    viewModel.CurrentConversation = viewModel.Conversations[0];
                    
                    // Get messages for this conversation
                    string messagesKey = $"Messages_{viewModel.CurrentConversation.Id}";
                    string messagesJson = HttpContext.Session.GetString(messagesKey);
                    
                    if (!string.IsNullOrEmpty(messagesJson))
                    {
                        var sessionMessages = JsonConvert.DeserializeObject<List<MessageViewModel>>(messagesJson);
                        
                        // Convert MessageViewModel to Message
                        viewModel.Messages = sessionMessages.Select(m => new MarketMinds.Shared.Models.Message
                        {
                            Id = m.Id,
                            ConversationId = m.ConversationId,
                            UserId = m.UserId,
                            Content = m.Content
                        }).ToList();
                    }
                    else
                    {
                        // Create empty message list
                        viewModel.Messages = new List<MarketMinds.Shared.Models.Message>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat data: {Message}", ex.Message);
                // Continue with empty data
            }
            
            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConversation()
        {
            _logger.LogInformation("Creating a new conversation");
            
            try
            {
                // Get current user ID
                int userId = User.Identity.IsAuthenticated ? User.GetCurrentUserId() : 1;
                
                // Get existing conversations from session
                string conversationsJson = HttpContext.Session.GetString("UserConversations");
                List<MarketMinds.Shared.Models.Conversation> userConversations;
                
                if (!string.IsNullOrEmpty(conversationsJson))
                {
                    userConversations = JsonConvert.DeserializeObject<List<MarketMinds.Shared.Models.Conversation>>(conversationsJson);
                }
                else
                {
                    userConversations = new List<MarketMinds.Shared.Models.Conversation>();
                }
                
                // Generate a new conversation ID (making sure it's unique)
                int newId = 1;
                if (userConversations.Any())
                {
                    newId = userConversations.Max(c => c.Id) + 1;
                }
                
                // Create a new conversation
                var newConversation = new MarketMinds.Shared.Models.Conversation
                {
                    Id = newId,
                    UserId = userId
                };
                
                // Add to the list and save to session
                userConversations.Add(newConversation);
                HttpContext.Session.SetString("UserConversations", JsonConvert.SerializeObject(userConversations));
                
                _logger.LogInformation("Created new conversation with ID: {ConversationId}", newId);
                
                return RedirectToAction("Index", new { conversationId = newId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation: {Message}", ex.Message);
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            _logger.LogInformation("Sending message in conversation {ConversationId}: {Content}", conversationId, content);
            
            try
            {
                // Get current user ID
                int userId = User.Identity.IsAuthenticated ? User.GetCurrentUserId() : 1;
                
                // Get existing messages from session
                string messagesKey = $"Messages_{conversationId}";
                string messagesJson = HttpContext.Session.GetString(messagesKey);
                List<MessageViewModel> messages;
                
                if (!string.IsNullOrEmpty(messagesJson))
                {
                    messages = JsonConvert.DeserializeObject<List<MessageViewModel>>(messagesJson);
                }
                else
                {
                    messages = new List<MessageViewModel>();
                }
                
                // Add the user's message
                messages.Add(new MessageViewModel
                {
                    Id = messages.Count + 1,
                    ConversationId = conversationId,
                    UserId = userId,
                    Content = content,
                    IsFromUser = true
                });
                
                // Try to get a bot response
                try
                {
                    string botResponse = await _chatbotService.GetBotResponseAsync(content);
                    
                    // Add the bot's response
                    messages.Add(new MessageViewModel
                    {
                        Id = messages.Count + 1,
                        ConversationId = conversationId,
                        UserId = 0, // Bot user ID
                        Content = botResponse ?? "I'm sorry, I couldn't process your request.",
                        IsFromUser = false
                    });
                    
                    _logger.LogInformation("Bot response: {Response}", botResponse);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting bot response: {Message}", ex.Message);
                    
                    // Add a fallback response if the bot service fails
                    messages.Add(new MessageViewModel
                    {
                        Id = messages.Count + 1,
                        ConversationId = conversationId,
                        UserId = 0, // Bot user ID
                        Content = "I'm sorry, I'm having trouble understanding right now. Please try again later.",
                        IsFromUser = false
                    });
                }
                
                // Save updated messages to session
                HttpContext.Session.SetString(messagesKey, JsonConvert.SerializeObject(messages));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message: {Message}", ex.Message);
            }
            
            return RedirectToAction("Index", new { conversationId });
        }
    }
}
