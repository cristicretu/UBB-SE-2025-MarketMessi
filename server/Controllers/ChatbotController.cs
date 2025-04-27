using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace server.Controllers
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
        private static bool _startupDebugComplete = false;

        public ChatbotController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ChatbotController> logger)
        {
            this.configuration = configuration;
            httpClient = httpClientFactory.CreateClient();
            geminiApiKey = this.configuration["GeminiAPI:Key"];
            
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
                
                var geminiRequest = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
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
    }

    public class ChatbotRequest
    {
        public string Message { get; set; }
    }
    public class ChatbotResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
