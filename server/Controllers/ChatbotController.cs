using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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

        public ChatbotController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            httpClient = httpClientFactory.CreateClient();
            geminiApiKey = this.configuration["GeminiAPI:Key"];
        }

        [HttpPost]
        public async Task<IActionResult> GetChatbotResponse([FromBody] ChatbotRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest("Message cannot be empty");
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
                var response = await httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { error = $"API Error: {responseBody}" });
                }

                using (JsonDocument document = JsonDocument.Parse(responseBody))
                {
                    try 
                    {
                        var candidates = document.RootElement.GetProperty("candidates");
                        if (candidates.GetArrayLength() > 0)
                        {
                            var contentElement = candidates[0].GetProperty("content");
                            var parts = contentElement.GetProperty("parts");
                            if (parts.GetArrayLength() > 0)
                            {
                                var text = parts[0].GetProperty("text").GetString();
                                
                                if (!string.IsNullOrEmpty(text))
                                {
                                    return Ok(new ChatbotResponse
                                    {
                                        Message = text,
                                        Success = true
                                    });
                                }
                            }
                        }
                        
                        throw new JsonException("Could not extract text from Gemini API response");
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { error = "Invalid response format from Gemini API", details = ex.Message });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error: {ex.Message}" });
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
