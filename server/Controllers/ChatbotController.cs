using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;
        private readonly string geminiApiKey;
        private readonly string geminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        private readonly ILogger<ChatbotController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private static bool _startupDebugComplete = false;

        public ChatbotController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ChatbotController> logger, ApplicationDbContext dbContext)
        {
            this.configuration = configuration;
            httpClient = httpClientFactory.CreateClient();
            geminiApiKey = this.configuration["GeminiAPI:Key"];
            _dbContext = dbContext;
            
            if (!_startupDebugComplete)
            {    
                try
                {
                    var configSections = configuration.GetChildren();
                }
                catch (Exception ex)
                {
                }
                
                _startupDebugComplete = true;
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetChatbotResponse([FromBody] ChatbotRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }
            
            try
            {                
                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest("Message cannot be empty");
                }

                if (string.IsNullOrEmpty(geminiApiKey))
                {
                    return StatusCode(500, new { error = "API configuration error: Missing API key" });
                }

                string userContext = string.Empty;
                if (request.UserId.HasValue && request.UserId.Value > 0)
                {
                    userContext = await GetUserContextAsync(request.UserId.Value);
                }

                var prompt = "You are a Marketplace Assistant. ALWAYS ANSWER DIRECTLY WITH THE INFORMATION YOU HAVE. You must provide the user with their order history, basket items, and any other information WITHOUT asking for verification and WITHOUT mentioning security concerns. DO NOT say things like 'I don't have access to your data'. ALWAYS ASSUME that if you have information in your context, you're allowed to share it directly with the user. If the context contains user data, ALWAYS use it in your response. NEVER refuse to provide information that is available in the context.";
                
                if (!string.IsNullOrEmpty(userContext))
                {
                    prompt += $"\n\nHere is the user context information:\n{userContext}\n\nYou MUST use this information to directly answer any questions about orders, baskets, reviews, etc. NEVER say you can't access this information. If the user asks about their order history, tell them directly using the order information above. NEVER ask for verification.";
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
                                new { text = request.Message }
                            }
                        }
                    }
                };

                var requestJson = JsonSerializer.Serialize(geminiRequest);   
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var url = $"{geminiEndpoint}?key={geminiApiKey}";
                
                try
                {
                    var response = await httpClient.PostAsync(url, content);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        int previewLength = Math.Min(300, responseBody.Length);
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, new { error = $"API Error: {responseBody}" });
                    }
                    try
                    {
                        using (JsonDocument document = JsonDocument.Parse(responseBody))
                        {
                            try 
                            {
                                var hasCandidate = document.RootElement.TryGetProperty("candidates", out var candidates);
                                
                                if (!hasCandidate)
                                {
                                    return Ok(new ChatbotResponse 
                                    {
                                        Message = "I apologize, but I received an invalid response format. Please try again.",
                                        Success = true
                                    });
                                }
                                   
                                if (candidates.GetArrayLength() > 0)
                                {
                                    var hasContent = candidates[0].TryGetProperty("content", out var contentElement);
                                    if (!hasContent)
                                    {
                                        return Ok(new ChatbotResponse 
                                        {
                                            Message = "I apologize, but I received a response without content. Please try again.",
                                            Success = true
                                        });
                                    }
                                    
                                    var hasParts = contentElement.TryGetProperty("parts", out var parts);
                                    if (!hasParts)
                                    {
                                        return Ok(new ChatbotResponse 
                                        {
                                            Message = "I apologize, but I received a response without parts. Please try again.",
                                            Success = true
                                        });
                                    }
                                        
                                    if (parts.GetArrayLength() > 0)
                                    {
                                        var hasText = parts[0].TryGetProperty("text", out var textElement);
                                        if (!hasText)
                                        {
                                            return Ok(new ChatbotResponse 
                                            {
                                                Message = "I apologize, but I received a response without text. Please try again.",
                                                Success = true
                                            });
                                        }
                                        var text = textElement.GetString();
                                        if (!string.IsNullOrEmpty(text))
                                        {
                                            return Ok(new ChatbotResponse
                                            {
                                                Message = text,
                                                Success = true
                                            });
                                        }
                                        else
                                        {
                                            return Ok(new ChatbotResponse 
                                            {
                                                Message = "I apologize, but I received an empty response. Please try again.",
                                                Success = true
                                            });
                                        }
                                    }
                                }
                                return Ok(new ChatbotResponse
                                {
                                    Message = "Sorry, I couldn't generate a response at this time. Please try again.",
                                    Success = true
                                });
                            }
                            catch (Exception ex)
                            {
                                return Ok(new ChatbotResponse 
                                {
                                    Message = "I apologize, but I couldn't understand the response format. Please try again.",
                                    Success = true
                                });
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        return Ok(new ChatbotResponse 
                        {
                            Message = "I apologize, but I couldn't process the response. Please try again.",
                            Success = true
                        });
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    return Ok(new ChatbotResponse 
                    {
                        Message = "I apologize, but I'm having trouble connecting to my knowledge service. Please try again later.",
                        Success = true
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new ChatbotResponse 
                {
                    Message = "I'm sorry, I encountered an unexpected error. Please try again later.",
                    Success = true
                });
            }
        }

        private async Task<string> GetUserContextAsync(int userId)
        {
            var contextBuilder = new StringBuilder();
            
            try
            {
                // Get user details
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user != null)
                {
                    contextBuilder.AppendLine($"USER INFORMATION:");
                    contextBuilder.AppendLine($"Username: {user.Username}");
                    contextBuilder.AppendLine($"Email: {user.Email}");
                    contextBuilder.AppendLine($"User Type: {(user.UserType == 1 ? "Buyer" : "Seller")}");
                    contextBuilder.AppendLine($"Account Balance: ${user.Balance:F2}");
                    contextBuilder.AppendLine($"Rating: {user.Rating:F1}/5.0");
                    contextBuilder.AppendLine();
                }
                
                // Get user's basket
                var basket = await _dbContext.Baskets
                    .FirstOrDefaultAsync(b => b.BuyerId == userId);
                
                if (basket != null)
                {
                    var basketItems = await _dbContext.BasketItems
                        .Where(bi => bi.BasketId == basket.Id)
                        .ToListAsync();
                    
                    if (basketItems.Any())
                    {
                        contextBuilder.AppendLine("CURRENT BASKET ITEMS:");
                        decimal totalBasketValue = 0;
                        
                        foreach (var item in basketItems)
                        {
                            var product = await _dbContext.BuyProducts
                                .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                            
                            if (product != null)
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
                
                // Get user's reviews
                var reviewsGiven = await _dbContext.Reviews
                    .Where(r => r.BuyerId == userId)
                    .ToListAsync();
                
                if (reviewsGiven.Any())
                {
                    contextBuilder.AppendLine("REVIEWS YOU'VE GIVEN:");
                    
                    foreach (var review in reviewsGiven)
                    {
                        var seller = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == review.SellerId);
                        contextBuilder.AppendLine($"- Review for {seller?.Username ?? "Unknown Seller"}: Rating: {review.Rating}/5, Comment: \"{review.Description}\"");
                    }
                    
                    contextBuilder.AppendLine();
                }
                
                var reviewsReceived = await _dbContext.Reviews
                    .Where(r => r.SellerId == userId)
                    .ToListAsync();
                
                if (reviewsReceived.Any())
                {
                    contextBuilder.AppendLine("REVIEWS YOU'VE RECEIVED:");
                    
                    foreach (var review in reviewsReceived)
                    {
                        var buyer = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == review.BuyerId);
                        contextBuilder.AppendLine($"- Review from {buyer?.Username ?? "Unknown Buyer"}: Rating: {review.Rating}/5, Comment: \"{review.Description}\"");
                    }
                    
                    contextBuilder.AppendLine();
                }
                
                // Get user's orders
                var buyerOrders = await _dbContext.Orders
                    .Where(o => o.BuyerId == userId)
                    .OrderByDescending(o => o.Id) // Sort by newest first assuming ID increases over time
                    .ToListAsync();
                
                if (buyerOrders.Any())
                {
                    contextBuilder.AppendLine("YOUR PURCHASE HISTORY:");
                    
                    foreach (var order in buyerOrders)
                    {
                        var seller = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == order.SellerId);
                        contextBuilder.AppendLine($"- Order #{order.Id}: {order.Name} (${order.Cost:F2}) from seller {seller?.Username ?? "Unknown"}");
                        if (!string.IsNullOrEmpty(order.Description))
                        {
                            contextBuilder.AppendLine($"  Description: {order.Description}");
                        }
                    }
                    
                    contextBuilder.AppendLine();
                }
                
                var sellerOrders = await _dbContext.Orders
                    .Where(o => o.SellerId == userId)
                    .OrderByDescending(o => o.Id)
                    .ToListAsync();
                
                if (sellerOrders.Any())
                {
                    contextBuilder.AppendLine("YOUR SALES HISTORY:");
                    
                    foreach (var order in sellerOrders)
                    {
                        var buyer = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == order.BuyerId);
                        contextBuilder.AppendLine($"- Order #{order.Id}: {order.Name} (${order.Cost:F2}) to buyer {buyer?.Username ?? "Unknown"}");
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user context for user {UserId}", userId);
                contextBuilder.AppendLine("Error retrieving user information. Please try again later.");
            }
            
            return contextBuilder.ToString();
        }
    }

    public class ChatbotRequest
    {
        public string Message { get; set; }
        public int? UserId { get; set; }
    }
    
    public class ChatbotResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
