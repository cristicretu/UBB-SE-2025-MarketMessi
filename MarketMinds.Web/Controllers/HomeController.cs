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
        public async Task<IActionResult> Create(AuctionProduct auctionProduct, string productType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set default values
                    auctionProduct.StartTime = DateTime.Now;
                    auctionProduct.EndTime = DateTime.Now.AddDays(7);
                    auctionProduct.CurrentPrice = auctionProduct.StartPrice;
                    
                    // Create the product
                    var result = await _auctionProductService.CreateAuctionProductAsync(auctionProduct);
                    
                    if (result)
                    {
                        return RedirectToAction("Index", "AuctionProducts");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create product.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the product.");
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
            var errorModel = new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = TempData["ErrorMessage"] as string ?? "An unexpected error occurred"
            };
            
            _logger.LogInformation("Displaying error page. RequestId: {RequestId}, Message: {Message}", 
                errorModel.RequestId, errorModel.ErrorMessage);
                
            return View(errorModel);
        }
    }
} 