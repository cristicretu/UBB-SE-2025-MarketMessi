using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;
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
        private readonly ILogger<AuctionProductsController> _logger;
        private readonly IAuctionProductService _auctionProductService;

        public AuctionProductsController(
            ILogger<AuctionProductsController> logger,
            IAuctionProductService auctionProductService)
        {
            _logger = logger;
            _auctionProductService = auctionProductService;
        }

        // GET: AuctionProducts
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all auction products");
                var auctionProducts = await _auctionProductService.GetAllAuctionProductsAsync();
                return View(auctionProducts);
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
                _logger.LogInformation($"Fetching auction product with ID {id}");
                var auctionProduct = await _auctionProductService.GetAuctionProductByIdAsync(id);
                
                if (auctionProduct == null || auctionProduct.Id == 0)
                {
                    _logger.LogWarning($"Auction product with ID {id} not found");
                    return NotFound();
                }
                
                return View(auctionProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching auction product {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: AuctionProducts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AuctionProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuctionProduct auctionProduct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _auctionProductService.CreateAuctionProductAsync(auctionProduct);
                    if (result)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating auction product");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the auction product");
                }
            }
            return View(auctionProduct);
        }

        // GET: AuctionProducts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var auctionProduct = await _auctionProductService.GetAuctionProductByIdAsync(id);
            if (auctionProduct == null || auctionProduct.Id == 0)
            {
                return NotFound();
            }
            return View(auctionProduct);
        }

        // POST: AuctionProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AuctionProduct auctionProduct)
        {
            if (id != auctionProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _auctionProductService.UpdateAuctionProductAsync(auctionProduct);
                    if (result)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating auction product {id}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the auction product");
                }
            }
            return View(auctionProduct);
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