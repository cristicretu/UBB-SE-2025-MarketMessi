using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MarketMinds.Web.Controllers
{
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
                PropertyNameCaseInsensitive = true
            };
        }

        // GET: AuctionProducts
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _clientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync("auctionproducts");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var auctionProducts = JsonSerializer.Deserialize<List<AuctionProduct>>(content, _jsonOptions);
                    return View(auctionProducts);
                }
                else
                {
                    _logger.LogError($"API returned {response.StatusCode} when getting auction products");
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
                var response = await client.GetAsync($"auctionproducts/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var auctionProduct = JsonSerializer.Deserialize<AuctionProduct>(content, _jsonOptions);
                    return View(auctionProduct);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError($"API returned {response.StatusCode} when getting auction product {id}");
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