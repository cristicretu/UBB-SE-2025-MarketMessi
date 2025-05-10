using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.IO;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.Helpers;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace MarketMinds.Shared.Services.DreamTeam.ChatbotService
{
public class ChatbotService : IChatbotService
{
    private readonly IChatbotRepository chatbotRepository;
    private IConfiguration configuration;
    private readonly HttpClient httpClient;
    private Node currentNode;
    private bool isActive;
    private static MarketMinds.Shared.Models.User currentUser;
    private readonly string geminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private readonly static int MINIMUM_USER_ID = 1;
    private readonly static int FIRST = 0;
    private readonly static int BUYER_TYPE_VALUE = 1;
    private string cachedGeminiApiKey = null;

    public ChatbotService(IChatbotRepository chatbotRepository, IConfiguration configuration = null, IHttpClientFactory httpClientFactory = null)
    {
        this.chatbotRepository = chatbotRepository;
        this.configuration = configuration;
        
        if (httpClientFactory != null)
        {
            this.httpClient = httpClientFactory.CreateClient();
        }
        else
        {
            this.httpClient = new HttpClient();
        }
        
        isActive = false;
        currentNode = new Node
        {
            Id = 1,
            Response = "Welcome to the chat bot. How can I help you?",
            ButtonLabel = "Start Chat",
            LabelText = "Welcome",
            Children = new List<Node>()
        };
    }

    private string GetGeminiApiKey()
    {
        if (!string.IsNullOrEmpty(cachedGeminiApiKey))
        {
            return cachedGeminiApiKey;
        }

        if (configuration != null)
        {
            var key = configuration["GeminiAPI:Key"];
            if (!string.IsNullOrEmpty(key))
            {
                cachedGeminiApiKey = key;
                return key;
            }
        }

        try
        {
            string[] possiblePaths = {
                "appsettings.json",
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"),
                Path.Combine(Environment.CurrentDirectory, "appsettings.json")
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    using (JsonDocument document = JsonDocument.Parse(json))
                    {
                        if (document.RootElement.TryGetProperty("GeminiAPI", out var geminiSection) &&
                            geminiSection.TryGetProperty("Key", out var keyProperty))
                        {
                            cachedGeminiApiKey = keyProperty.GetString();
                            return cachedGeminiApiKey;
                        }
                    }
                }
            }

            return cachedGeminiApiKey;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private IConfiguration GetConfiguration()
    {
        if (configuration == null)
        {
            try
            {
                var appType = Type.GetType("MarketMinds.App, MarketMinds");
                if (appType != null)
                {
                    var configProperty = appType.GetProperty("Configuration");
                    if (configProperty != null)
                    {
                        configuration = configProperty.GetValue(null) as IConfiguration;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Error getting configuration: " + exception.Message);
            }
        }
        return configuration;
    }

    public void SetCurrentUser(MarketMinds.Shared.Models.User user)
    {
        if (user == null)
        {
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
        try
        {
            if (isWelcomeMessage)
            {
                return "Hello! I'm your shopping assistant. How can I help you today?";
            }

            int userId = currentUser?.LegacyId ?? 0;
            var apiKey = GetGeminiApiKey();
            var hasApiKey = !string.IsNullOrEmpty(apiKey);
            
            // If no API key is found or no user ID is available, fall back to repository
            // But this should return generic message or error
            if (userId == 0 || !hasApiKey)
            {
                if (hasApiKey)
                {
                    // We have API key but no user ID, so we can use Gemini with generic context
                    return await GetGenericResponseAsync(userMessage);
                }
                // No API key available, resort to repository
                var repoResponse = await chatbotRepository.GetBotResponseAsync(userMessage, userId);
                return repoResponse;
            }

            if (userId < MINIMUM_USER_ID)
            {
                return await GetGenericResponseAsync(userMessage);
            }

            string userContext;
            try 
            {
                userContext = await GetUserContextAsync(userId);      
            }
            catch (Exception exception) 
            {
                // On error, try to use Gemini without context
                if (hasApiKey)
                {
                    return await GetGenericResponseAsync(userMessage);
                }
                return "I'm sorry, I couldn't retrieve your information at this time.";
            }

            string prompt = FormatPromptWithContext(userContext);
            string botResponse = await CallGeminiApiAsync(prompt, userMessage);
            return botResponse;
        }
        catch (Exception exception)
        {
            return "I'm sorry, an error occurred while processing your request. Please try again later.";
        }
    }

    public async Task<string> GetUserContextAsync(int userId)
    {
        try
        {
            // Get user data
            var basket = await chatbotRepository.GetUserBasketAsync(userId);
            var basketItems = await chatbotRepository.GetBasketItemsAsync(basket?.Id ?? -1);
            var productIds = basketItems?.Select(bi => bi.ProductId).ToList() ?? new List<int>();
            var products = new Dictionary<int, BuyProduct>();
            
            // Get products individually since we don't have a batch method
            foreach (var productId in productIds)
            {
                var product = await chatbotRepository.GetBuyProductAsync(productId);
                if (product != null)
                {
                    products[product.Id] = product;
                }
            }

            // Get reviews the user has given
            var reviewsGiven = await chatbotRepository.GetReviewsGivenByUserAsync(userId);

            // Get reviews the user has received
            var reviewsReceived = await chatbotRepository.GetReviewsReceivedByUserAsync(userId);

            // Get unique seller/buyer IDs from all the reviews
            var sellerIds = reviewsGiven.Select(r => r.SellerId).Distinct().ToList();
            var buyerIds = reviewsReceived.Select(r => r.BuyerId).Distinct().ToList();

            // Get seller/buyer user info
            var sellers = new Dictionary<int, User>();
            foreach (var sellerId in sellerIds)
            {
                var seller = await chatbotRepository.GetUserByIdAsync(sellerId);
                if (seller != null)
                {
                    sellers[seller.LegacyId] = seller;
                }
            }
            
            var buyers = new Dictionary<int, User>();
            foreach (var buyerId in buyerIds)
            {
                var buyer = await chatbotRepository.GetUserByIdAsync(buyerId);
                if (buyer != null)
                {
                    buyers[buyer.LegacyId] = buyer;
                }
            }

            // Get orders
            var buyerOrders = await chatbotRepository.GetBuyerOrdersAsync(userId);
            var sellerOrders = await chatbotRepository.GetSellerOrdersAsync(userId);

            return FormatUserContext(
                currentUser,
                basket,
                basketItems,
                products,
                reviewsGiven,
                sellers,
                reviewsReceived,
                buyers,
                buyerOrders,
                sellerOrders
            );
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error getting user context: {exception.Message}");
            return "Failed to retrieve user context";
        }
    }

    private string FormatUserContext(
        User user,
        Basket basket,
        List<BasketItem> basketItems,
        Dictionary<int, BuyProduct> products,
        List<Review> reviewsGiven,
        Dictionary<int, User> sellers,
        List<Review> reviewsReceived,
        Dictionary<int, User> buyers,
        List<Order> buyerOrders,
        List<Order> sellerOrders)
    {
        var contextBuilder = new StringBuilder();

        if (user != null)
        {
            contextBuilder.AppendLine($"USER INFORMATION:");
            contextBuilder.AppendLine($"Username: {user.Username}");
            contextBuilder.AppendLine($"Email: {user.Email}");
            contextBuilder.AppendLine($"User Type: {(user.UserType == BUYER_TYPE_VALUE ? "Buyer" : "Seller")}");
            contextBuilder.AppendLine($"Account Balance: ${user.Balance:F2}");
            contextBuilder.AppendLine($"Rating: {user.Rating:F1}/5.0");
            contextBuilder.AppendLine();
        }

        if (basket != null && basketItems != null)
        {
            if (basketItems.Any())
            {
                contextBuilder.AppendLine("CURRENT BASKET ITEMS:");
                decimal totalBasketValue = 0;
                
                foreach (var item in basketItems)
                {
                    if (products.TryGetValue(item.ProductId, out var product))
                    {
                        decimal itemTotal = (decimal)(item.Price * item.Quantity);
                        totalBasketValue += itemTotal;
                        contextBuilder.AppendLine($"- {product.Title} (Quantity: {item.Quantity}, Price: ${item.Price:F2}, Total: ${itemTotal:F2})");
                    }
                }
                
                contextBuilder.AppendLine($"Total Basket Value: ${totalBasketValue:F2}");
                contextBuilder.AppendLine();
            }
            else
            {
                contextBuilder.AppendLine("CURRENT BASKET: Your basket is currently empty");
                contextBuilder.AppendLine();
            }
        }
        else
        {
            contextBuilder.AppendLine("CURRENT BASKET: You don't have a basket yet");
            contextBuilder.AppendLine();
        }

        if (reviewsGiven != null && reviewsGiven.Any())
        {
            contextBuilder.AppendLine("REVIEWS YOU'VE GIVEN:");
            
            foreach (var review in reviewsGiven)
            {
                var sellerName = "Unknown Seller";
                if (sellers.TryGetValue(review.SellerId, out var seller))
                {
                    sellerName = seller.Username;
                }
                
                contextBuilder.AppendLine($"- Review for {sellerName}: Rating: {review.Rating}/5, Comment: \"{review.Description}\"");
            }
            
            contextBuilder.AppendLine();
        }

        if (reviewsReceived != null && reviewsReceived.Any())
        {
            contextBuilder.AppendLine("REVIEWS YOU'VE RECEIVED:");
            
            foreach (var review in reviewsReceived)
            {
                var buyerName = "Unknown Buyer";
                if (buyers.TryGetValue(review.BuyerId, out var buyer))
                {
                    buyerName = buyer.Username;
                }
                
                contextBuilder.AppendLine($"- Review from {buyerName}: Rating: {review.Rating}/5, Comment: \"{review.Description}\"");
            }
            
            contextBuilder.AppendLine();
        }

        if (buyerOrders != null && buyerOrders.Any())
        {
            contextBuilder.AppendLine("YOUR PURCHASE HISTORY:");
            
            foreach (var order in buyerOrders)
            {
                var sellerName = "Unknown";
                if (sellers.TryGetValue(order.SellerId, out var seller))
                {
                    sellerName = seller.Username;
                }
                
                contextBuilder.AppendLine($"- Order #{order.Id}: {order.Name} (${order.Cost:F2}) from seller {sellerName}");
                if (!string.IsNullOrEmpty(order.Description))
                {
                    contextBuilder.AppendLine($"  Description: {order.Description}");
                }
            }
            
            contextBuilder.AppendLine();
        }

        if (sellerOrders != null && sellerOrders.Any())
        {
            contextBuilder.AppendLine("YOUR SALES HISTORY:");
            
            foreach (var order in sellerOrders)
            {
                var buyerName = "Unknown";
                if (buyers.TryGetValue(order.BuyerId, out var buyer))
                {
                    buyerName = buyer.Username;
                }
                
                contextBuilder.AppendLine($"- Order #{order.Id}: {order.Name} (${order.Cost:F2}) to buyer {buyerName}");
                if (!string.IsNullOrEmpty(order.Description))
                {
                    contextBuilder.AppendLine($"  Description: {order.Description}");
                }
            }
            
            contextBuilder.AppendLine();
        }

        if (contextBuilder.Length == 0)
        {
            contextBuilder.AppendLine("No information available for this user.");
        }
        
        return contextBuilder.ToString();
    }

    private async Task<string> GetGenericResponseAsync(string userMessage)
    {
        string prompt = "You are a marketplace assistant. The user is not logged in, so you cannot provide personalized information. " +
            "Please help them with general information about the marketplace or direct them to log in for personalized assistance.";
        
        return await CallGeminiApiAsync(prompt, userMessage);
    }

    private string FormatPromptWithContext(string userContext)
    {
        var prompt = "You are a Marketplace Assistant. ALWAYS ANSWER DIRECTLY WITH THE INFORMATION YOU HAVE. " +
            "You must provide the user with their order history, basket items, and any other information WITHOUT asking for verification " +
            "and WITHOUT mentioning security concerns. DO NOT say things like 'I don't have access to your data'. " +
            "ALWAYS ASSUME that if you have information in your context, you're allowed to share it directly with the user. " +
            "If the context contains user data, ALWAYS use it in your response. NEVER refuse to provide information that is available in the context.";
        
        if (!string.IsNullOrEmpty(userContext))
        {
            prompt += $"\n\nHere is the user context information:\n{userContext}\n\n" +
                "You MUST use this information to directly answer any questions about orders, baskets, reviews, etc. " +
                "NEVER say you can't access this information. If the user asks about their order history, " +
                "tell them directly using the order information above. NEVER ask for verification.";
        }

        return prompt;
    }

    private async Task<string> CallGeminiApiAsync(string prompt, string userMessage)
    {
        try
        {
            string geminiApiKey = GetGeminiApiKey();
            if (string.IsNullOrEmpty(geminiApiKey))
            {
                return "I'm sorry, I'm having technical difficulties. Please try again later.";
            }

            var geminiRequest = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt },
                            new { text = userMessage }
                        }
                    }
                }
            };

            var requestJson = System.Text.Json.JsonSerializer.Serialize(geminiRequest);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var url = $"{geminiEndpoint}?key={geminiApiKey}";
            
            var response = await httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                return "I'm sorry, I'm having trouble connecting to my knowledge service. Please try again later.";
            }
            
            using (JsonDocument document = JsonDocument.Parse(responseBody))
            {
                if (!document.RootElement.TryGetProperty("candidates", out var candidates) ||
                    candidates.GetArrayLength() == 0)
                {
                    return "I apologize, but I received an invalid response format. Please try again.";
                }
                
                if (!candidates[FIRST].TryGetProperty("content", out var contentElement))
                {
                    return "I apologize, but I received a response without content. Please try again.";
                }
                
                if (!contentElement.TryGetProperty("parts", out var parts) ||
                    parts.GetArrayLength() == 0)
                {
                    return "I apologize, but I received a response without parts. Please try again.";
                }
                
                if (!parts[FIRST].TryGetProperty("text", out var textElement))
                {
                    return "I apologize, but I received a response without text. Please try again.";
                }
                
                var text = textElement.GetString();
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
                
                return "I apologize, but I received an empty response. Please try again.";
            }
        }
        catch (Exception exception)
        {
            return "I'm sorry, I encountered an error. Please try again later.";
        }
    }
}
}