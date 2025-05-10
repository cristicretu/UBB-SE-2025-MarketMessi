using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MarketMinds.Web.Models;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.ProductTagService;
using MarketMinds.Shared.Services.ProductCategoryService;
using MarketMinds.Shared.Services.ProductConditionService;
using MarketMinds.Shared.Services.ImagineUploadService;
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
        private readonly IProductTagService _productTagService;
        private readonly IProductCategoryService _categoryService;
        private readonly IProductConditionService _conditionService;
        private readonly IImageUploadService _imageUploadService;

        public HomeController(
            ILogger<HomeController> logger,
            IAuctionProductService auctionProductService,
            IProductTagService productTagService,
            IProductCategoryService categoryService,
            IProductConditionService conditionService,
            IImageUploadService imageUploadService)
        {
            _logger = logger;
            _auctionProductService = auctionProductService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _conditionService = conditionService;
            _imageUploadService = imageUploadService;
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
        public async Task<IActionResult> Create(AuctionProduct auctionProduct, string productType, string tagIds, string imageUrls)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Creating a new auction product");
                    
                    // Process tags if provided
                    var productTags = new List<ProductTag>();
                    if (!string.IsNullOrEmpty(tagIds))
                    {
                        _logger.LogInformation("Processing tags: {TagIds}", tagIds);
                        var tagIdList = tagIds.Split(',');
                        foreach (var tagId in tagIdList)
                        {
                            if (tagId.StartsWith("new_"))
                            {
                                // This is a new tag to be created
                                var tagTitle = tagId.Substring(4); // Remove "new_" prefix
                                _logger.LogInformation("Creating new tag: {TagTitle}", tagTitle);
                                var newTag = _productTagService.CreateProductTag(tagTitle);
                                productTags.Add(newTag);
                            }
                            else if (int.TryParse(tagId, out int existingTagId))
                            {
                                // This is an existing tag
                                // Get the tag from the service
                                var allTags = _productTagService.GetAllProductTags();
                                var tag = allTags.FirstOrDefault(t => t.Id == existingTagId);
                                if (tag != null)
                                {
                                    productTags.Add(tag);
                                }
                            }
                        }
                    }
                    
                    // Process image URLs if provided
                    var productImages = new List<Image>();
                    if (!string.IsNullOrEmpty(imageUrls))
                    {
                        _logger.LogInformation("Processing image URLs");
                        productImages = _imageUploadService.ParseImagesString(imageUrls);
                        // Set the images to the product (implementation may vary based on your model)
                        auctionProduct.NonMappedImages = productImages.ToList();
                    }
                    
                    // Set default values if not provided
                    if (auctionProduct.StartTime == default)
                    {
                        auctionProduct.StartTime = DateTime.Now;
                    }
                    
                    if (auctionProduct.EndTime == default)
                    {
                        auctionProduct.EndTime = DateTime.Now.AddDays(7);
                    }
                    
                    // Ensure current price is set
                    auctionProduct.CurrentPrice = auctionProduct.StartPrice;
                    
                    // Create the product
                    var result = await _auctionProductService.CreateAuctionProductAsync(auctionProduct);
                    
                    if (result)
                    {
                        _logger.LogInformation("Auction product created successfully");
                        return RedirectToAction("Index", "AuctionProducts");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create auction product");
                        ModelState.AddModelError(string.Empty, "Failed to create product.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the product.");
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state when creating auction product: {Errors}", 
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }
            
            // If we get here, something went wrong
            // Reload categories, conditions, and tags for the view
            ViewBag.Categories = _categoryService.GetAllProductCategories();
            ViewBag.Conditions = _conditionService.GetAllProductConditions();
            ViewBag.Tags = _productTagService.GetAllProductTags();
            
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