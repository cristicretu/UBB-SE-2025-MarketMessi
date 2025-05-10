using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MarketMinds.Web.Models;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

namespace MarketMinds.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuctionProductService _auctionProductService;

        public HomeController(
            ILogger<HomeController> logger,
            IAuctionProductService auctionProductService)
        {
            _logger = logger;
            _auctionProductService = auctionProductService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/Account
        public IActionResult Account()
        {
            // This is a placeholder for future account management functionality
            return View();
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            return View(new AuctionProduct());
        }

        // POST: Home/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AuctionProduct auctionProduct, 
            string productType, 
            string tags, 
            string imageUrls, 
            IFormFileCollection imageUpload)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation($"Creating new {productType} product: {auctionProduct.Title}");
                    
                    // Set seller ID (in a real app, this would come from authentication)
                    auctionProduct.SellerId = 1; // Demo user ID
                    
                    // Handle the product based on its type
                    switch (productType.ToLower())
                    {
                        case "auction":
                            // Process image URLs if provided
                            if (!string.IsNullOrEmpty(imageUrls))
                            {
                                var urls = imageUrls.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(url => url.Trim())
                                    .Where(url => !string.IsNullOrEmpty(url));
                                
                                foreach (var url in urls)
                                {
                                    auctionProduct.Images.Add(new ProductImage { Url = url });
                                }
                            }
                            
                            // Create the product through the service
                            var result = await _auctionProductService.CreateAuctionProductAsync(auctionProduct);
                            
                            if (result)
                            {
                                return RedirectToAction("Index", "AuctionProducts");
                            }
                            break;
                            
                        case "borrow":
                        case "buy":
                            // For future implementation
                            _logger.LogInformation($"{productType} product creation not yet implemented");
                            ModelState.AddModelError(string.Empty, $"{productType} product creation is not yet available");
                            break;
                            
                        default:
                            ModelState.AddModelError(string.Empty, $"Unknown product type: {productType}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating {productType} product");
                    ModelState.AddModelError(string.Empty, $"An error occurred while creating the {productType} product");
                }
            }
            
            return View(auctionProduct);
        }

        // GET: Home/Basket
        public IActionResult Basket()
        {
            // This is a placeholder for future basket functionality
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 