using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MarketMinds.Web.Controllers
{
    public class NumberToStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return string.Empty;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64().ToString();
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }

            throw new JsonException($"Unable to convert {reader.TokenType} to string");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value ?? string.Empty);
        }
    }

    public class AuctionProductsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AuctionProductsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuctionProductsController(
            IHttpClientFactory clientFactory,
            ILogger<AuctionProductsController> logger,
            IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            
            // Add custom converter for handling password property
            _jsonOptions.Converters.Add(new NumberToStringConverter());
        }

        // GET: AuctionProducts
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _clientFactory.CreateClient("ApiClient");
                
                // Log the base address and configuration
                _logger.LogInformation($"API Base URL from config: {_configuration["ApiSettings:BaseUrl"]}");
                _logger.LogInformation($"HttpClient BaseAddress: {client.BaseAddress}");
                
                var requestUrl = "api/auctionproducts";
                _logger.LogInformation($"Requesting URL: {client.BaseAddress}{requestUrl}");
                
                var response = await client.GetAsync(requestUrl);
                _logger.LogInformation($"Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response content: {content.Substring(0, Math.Min(content.Length, 100))}...");
                    var auctionProducts = JsonSerializer.Deserialize<List<AuctionProduct>>(content, _jsonOptions);
                    return View(auctionProducts);
                }
                else
                {
                    _logger.LogError($"API returned {response.StatusCode} when getting auction products");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error response content: {errorContent}");
                    ModelState.AddModelError(string.Empty, "Unable to fetch auction products from the API");
                    return View(new List<AuctionProduct>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching auction products");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching auction products");
                return View(new List<AuctionProduct>());
            }
        }

        // GET: AuctionProducts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var client = _clientFactory.CreateClient("ApiClient");
                var requestUrl = $"api/auctionproducts/{id}";
                _logger.LogInformation($"Requesting URL: {client.BaseAddress}{requestUrl}");
                
                var response = await client.GetAsync(requestUrl);
                _logger.LogInformation($"Response status for id {id}: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var auctionProduct = JsonSerializer.Deserialize<AuctionProduct>(content, _jsonOptions);
                    return View(auctionProduct);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Auction product with ID {id} not found");
                    return NotFound();
                }
                else
                {
                    _logger.LogError($"API returned {response.StatusCode} when getting auction product {id}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error response content: {errorContent}");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching auction product {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to calculate time left for an auction
        [NonAction]
        public string GetTimeLeft(DateTime endTime)
        {
            var timeLeft = endTime - DateTime.Now;
            
            if (timeLeft <= TimeSpan.Zero)
            {
                return "Auction Ended";
            }
            
            return $"{timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m";
        }
    }
} 