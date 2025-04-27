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
            _logger = logger;
            
            // Log startup information only once
            if (!_startupDebugComplete)
            {
                Console.WriteLine("================================================");
                Console.WriteLine("CHATBOT CONTROLLER INITIALIZATION");
                Console.WriteLine("================================================");
                
                // Log all relevant configuration
                try
                {
                    var configSections = configuration.GetChildren();
                    Console.WriteLine("Configuration sections:");
                    foreach (var section in configSections)
                    {
                        Console.WriteLine($"- {section.Key}");
                    }
                    
                    Console.WriteLine($"GeminiAPI section exists: {configuration.GetSection("GeminiAPI") != null}");
                    
                    if (!string.IsNullOrEmpty(geminiApiKey))
                    {
                        Console.WriteLine($"Gemini API Key loaded successfully (first 4 chars): {geminiApiKey.Substring(0, 4)}...");
                    }
                    else
                    {
                        Console.WriteLine("*** ERROR: Gemini API Key is missing! ***");
                        Console.WriteLine($"Key from config: {configuration["GeminiAPI:Key"] ?? "null"}");
                    }
                    
                    Console.WriteLine($"Gemini API Endpoint: {geminiEndpoint}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception during config debug: {ex.Message}");
                }
                
                _startupDebugComplete = true;
                Console.WriteLine("================================================");
            }
            
            // Standard logging
            _logger.LogInformation("ChatbotController initialized");
            _logger.LogDebug($"Gemini API Endpoint: {geminiEndpoint}");
            _logger.LogDebug($"Gemini API Key present: {!string.IsNullOrEmpty(geminiApiKey)}");
            if (!string.IsNullOrEmpty(geminiApiKey))
            {
                _logger.LogDebug($"Gemini API Key first 4 chars: {geminiApiKey.Substring(0, 4)}..."); 
            }
            else
            {
                _logger.LogError("GeminiAPI:Key is missing from configuration!");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetChatbotResponse([FromBody] ChatbotRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("\n================================================");
            Console.WriteLine($"ChatbotController.GetChatbotResponse called at: {DateTime.Now:HH:mm:ss.fff}");
            
            if (request == null)
            {
                Console.WriteLine("ERROR: Request object is NULL");
                Console.WriteLine("================================================\n");
                return BadRequest("Request cannot be null");
            }
            
            Console.WriteLine($"Request message: '{request.Message}'");
            _logger.LogInformation($"***** ChatbotController.GetChatbotResponse called with message: '{request?.Message}' *****");
            
            try
            {                
                if (string.IsNullOrEmpty(request.Message))
                {
                    _logger.LogWarning("Empty message received");
                    Console.WriteLine("Empty message received");
                    Console.WriteLine("================================================\n");
                    return BadRequest("Message cannot be empty");
                }

                _logger.LogDebug("Preparing request to Gemini API");
                Console.WriteLine("Preparing request to Gemini API");
                
                // Check API key
                if (string.IsNullOrEmpty(geminiApiKey))
                {
                    _logger.LogError("Gemini API key is missing or empty!");
                    Console.WriteLine("Gemini API key is missing or empty!");
                    Console.WriteLine("================================================\n");
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
                _logger.LogDebug($"Request JSON: {requestJson}");
                Console.WriteLine($"Request JSON: {requestJson}");
                
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var url = $"{geminiEndpoint}?key={geminiApiKey}";
                
                _logger.LogDebug($"Sending request to: {geminiEndpoint}");
                Console.WriteLine($"Sending request to Gemini API with key starting with: {geminiApiKey.Substring(0, 4)}...");
                Console.WriteLine($"Elapsed time before API call: {stopwatch.ElapsedMilliseconds}ms");
                
                try
                {
                    Console.WriteLine($"Making HTTP POST to: {url}");
                    var response = await httpClient.PostAsync(url, content);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    
                    Console.WriteLine($"Response received in: {stopwatch.ElapsedMilliseconds}ms");
                    Console.WriteLine($"Response status: {response.StatusCode}");
                    Console.WriteLine($"Response length: {responseBody?.Length ?? 0}");
                    
                    // Log the first part of the response for debugging
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        int previewLength = Math.Min(300, responseBody.Length);
                        Console.WriteLine($"Response preview: {responseBody.Substring(0, previewLength)}...");
                    }
                    
                    _logger.LogDebug($"Response status: {response.StatusCode}");
                    _logger.LogDebug($"Response length: {responseBody?.Length ?? 0}");

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"API Error ({response.StatusCode}): {responseBody}");
                        Console.WriteLine($"API Error ({response.StatusCode}): {responseBody}");
                        Console.WriteLine("================================================\n");
                        return StatusCode((int)response.StatusCode, new { error = $"API Error: {responseBody}" });
                    }

                    _logger.LogDebug("Parsing Gemini response");
                    Console.WriteLine("Parsing Gemini response");
                    
                    // If we can't parse the response, use a simple fallback response
                    try
                    {
                        using (JsonDocument document = JsonDocument.Parse(responseBody))
                        {
                            try 
                            {
                                Console.WriteLine("Successfully parsed JSON document");
                                var hasCandidate = document.RootElement.TryGetProperty("candidates", out var candidates);
                                
                                if (!hasCandidate)
                                {
                                    Console.WriteLine("ERROR: 'candidates' property not found in response");
                                    Console.WriteLine("================================================\n");
                                    return Ok(new ChatbotResponse 
                                    {
                                        Message = "I apologize, but I received an invalid response format. Please try again.",
                                        Success = true
                                    });
                                }
                                
                                _logger.LogDebug($"Found {candidates.GetArrayLength()} candidates");
                                Console.WriteLine($"Found {candidates.GetArrayLength()} candidates");
                                
                                if (candidates.GetArrayLength() > 0)
                                {
                                    var hasContent = candidates[0].TryGetProperty("content", out var contentElement);
                                    if (!hasContent)
                                    {
                                        Console.WriteLine("ERROR: 'content' property not found in candidate");
                                        Console.WriteLine("================================================\n");
                                        return Ok(new ChatbotResponse 
                                        {
                                            Message = "I apologize, but I received a response without content. Please try again.",
                                            Success = true
                                        });
                                    }
                                    
                                    var hasParts = contentElement.TryGetProperty("parts", out var parts);
                                    if (!hasParts)
                                    {
                                        Console.WriteLine("ERROR: 'parts' property not found in content");
                                        Console.WriteLine("================================================\n");
                                        return Ok(new ChatbotResponse 
                                        {
                                            Message = "I apologize, but I received a response without parts. Please try again.",
                                            Success = true
                                        });
                                    }
                                    
                                    _logger.LogDebug($"Found {parts.GetArrayLength()} parts");
                                    Console.WriteLine($"Found {parts.GetArrayLength()} parts");
                                    
                                    if (parts.GetArrayLength() > 0)
                                    {
                                        var hasText = parts[0].TryGetProperty("text", out var textElement);
                                        if (!hasText)
                                        {
                                            Console.WriteLine("ERROR: 'text' property not found in part");
                                            Console.WriteLine("================================================\n");
                                            return Ok(new ChatbotResponse 
                                            {
                                                Message = "I apologize, but I received a response without text. Please try again.",
                                                Success = true
                                            });
                                        }
                                        
                                        var text = textElement.GetString();
                                        
                                        if (!string.IsNullOrEmpty(text))
                                        {
                                            _logger.LogInformation($"Returning successful response, length: {text.Length}");
                                            Console.WriteLine($"Returning successful response, first 50 chars: {text.Substring(0, Math.Min(50, text.Length))}...");
                                            Console.WriteLine($"Total processing time: {stopwatch.ElapsedMilliseconds}ms");
                                            Console.WriteLine("================================================\n");
                                            return Ok(new ChatbotResponse
                                            {
                                                Message = text,
                                                Success = true
                                            });
                                        }
                                        else
                                        {
                                            Console.WriteLine("ERROR: 'text' property is empty or null");
                                            Console.WriteLine("================================================\n");
                                            return Ok(new ChatbotResponse 
                                            {
                                                Message = "I apologize, but I received an empty response. Please try again.",
                                                Success = true
                                            });
                                        }
                                    }
                                }
                                
                                _logger.LogWarning("Could not extract text from Gemini API response");
                                Console.WriteLine("Could not extract text from Gemini API response");
                                Console.WriteLine("================================================\n");
                                return Ok(new ChatbotResponse
                                {
                                    Message = "Sorry, I couldn't generate a response at this time. Please try again.",
                                    Success = true
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Invalid response format from Gemini API");
                                Console.WriteLine($"Invalid response format from Gemini API: {ex.Message}");
                                Console.WriteLine("================================================\n");
                                
                                // Return a fallback response instead of failing completely
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
                        _logger.LogError(jsonEx, "Failed to parse response JSON");
                        Console.WriteLine($"Failed to parse response JSON: {jsonEx.Message}");
                        Console.WriteLine("================================================\n");
                        
                        // Return a fallback response instead of failing completely
                        return Ok(new ChatbotResponse 
                        {
                            Message = "I apologize, but I couldn't process the response. Please try again.",
                            Success = true
                        });
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    _logger.LogError(httpEx, "HTTP request to Gemini API failed");
                    Console.WriteLine($"HTTP request to Gemini API failed: {httpEx.Message}");
                    Console.WriteLine("================================================\n");
                    
                    // Return a fallback response instead of failing completely
                    return Ok(new ChatbotResponse 
                    {
                        Message = "I apologize, but I'm having trouble connecting to my knowledge service. Please try again later.",
                        Success = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatbotResponse");
                Console.WriteLine($"Error in GetChatbotResponse: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("================================================\n");
                
                // Return a fallback response instead of failing completely
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
