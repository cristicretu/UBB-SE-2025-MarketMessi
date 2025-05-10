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
using MarketMinds.Shared.Services.BorrowProductsService;

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
        private readonly IBorrowProductsService _borrowProductsService;

        public HomeController(
            ILogger<HomeController> logger,
            IAuctionProductService auctionProductService,
            IProductTagService productTagService,
            IProductCategoryService categoryService,
            IProductConditionService conditionService,
            IImageUploadService imageUploadService,
            IBorrowProductsService borrowProductsService)
        {
            _logger = logger;
            _auctionProductService = auctionProductService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _conditionService = conditionService;
            _imageUploadService = imageUploadService;
            _borrowProductsService = borrowProductsService;
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
            if (auctionProduct.StartTime == default)
            {
                auctionProduct.StartTime = DateTime.Now;
            }
            
            if (auctionProduct.EndTime == default)
            {
                auctionProduct.EndTime = DateTime.Now.AddDays(7);
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Creating a new auction product");
                    
                    var productTags = new List<ProductTag>();
                    if (!string.IsNullOrEmpty(tagIds))
                    {
                        _logger.LogInformation("Processing tags: {TagIds}", tagIds);
                        var tagIdList = tagIds.Split(',');
                        foreach (var tagId in tagIdList)
                        {
                            if (tagId.StartsWith("new_"))
                            {
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
                        _logger.LogInformation("Processing image URLs: {ImageUrls}", imageUrls);
                        productImages = _imageUploadService.ParseImagesString(imageUrls);
                        _logger.LogInformation("Parsed {ImageCount} images", productImages.Count);
                        // Set the images to the product (implementation may vary based on your model)
                        auctionProduct.NonMappedImages = productImages.ToList();
                    }
                    
                    if (auctionProduct.SellerId <= 0)
                    {
                        auctionProduct.SellerId = 1;
                    }
                    
                    if (auctionProduct.CurrentPrice <= 0)
                    {
                        auctionProduct.CurrentPrice = auctionProduct.StartPrice;
                    }
                    
                    // Log detailed product info before creating
                    _logger.LogInformation("Auction product details before creation: " +
                        "Id={Id}, Title={Title}, Description={DescriptionLength}, " +
                        "CategoryId={CategoryId}, ConditionId={ConditionId}, " +
                        "StartPrice={StartPrice}, CurrentPrice={CurrentPrice}, " +
                        "StartTime={StartTime}, EndTime={EndTime}, " +
                        "SellerId={SellerId}, TagCount={TagCount}, ImageCount={ImageCount}",
                        auctionProduct.Id,
                        auctionProduct.Title,
                        auctionProduct.Description?.Length ?? 0,
                        auctionProduct.CategoryId,
                        auctionProduct.ConditionId,
                        auctionProduct.StartPrice,
                        auctionProduct.CurrentPrice,
                        auctionProduct.StartTime,
                        auctionProduct.EndTime,
                        auctionProduct.SellerId,
                        productTags.Count,
                        productImages.Count);
                    
                    // Check required fields before submitting
                    if (string.IsNullOrWhiteSpace(auctionProduct.Title))
                    {
                        ModelState.AddModelError("Title", "Title is required");
                    }
                    
                    if (auctionProduct.CategoryId <= 0)
                    {
                        ModelState.AddModelError("CategoryId", "Please select a category");
                    }
                    
                    if (auctionProduct.ConditionId <= 0)
                    {
                        ModelState.AddModelError("ConditionId", "Please select a condition");
                    }
                    
                    if (auctionProduct.StartPrice <= 0)
                    {
                        ModelState.AddModelError("StartPrice", "Starting price must be greater than zero");
                    }
                    
                    if (ModelState.IsValid)
                    {
                        try 
                        {
                            var testProduct = await _auctionProductService.GetAuctionProductByIdAsync(1);
                            if (testProduct != null)
                            {
                                _logger.LogInformation("Service connectivity check: Successfully retrieved a test product");
                            }
                            else
                            {
                                _logger.LogWarning("Service connectivity check: Retrieved null test product");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Service connectivity check failed");
                        }
                        
                        bool result = false;
                        try 
                        {
                            result = await _auctionProductService.CreateAuctionProductAsync(auctionProduct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Exception thrown by CreateAuctionProductAsync");
                            
                            string errorMessage = ex.Message;
                            if (ex.InnerException != null)
                            {
                                if (ex.InnerException.Message.Contains("400 (Bad Request)"))
                                {
                                    errorMessage = "Please check that all required fields are filled in correctly.";
                                }
                                else
                                {
                                    errorMessage = ex.InnerException.Message;
                                }
                            }
                            
                            ModelState.AddModelError(string.Empty, $"Failed to create product: {errorMessage}");
                            throw;
                        }
                        
                        if (result)
                        {
                            _logger.LogInformation("Auction product created successfully");
                            return RedirectToAction("Index", "AuctionProducts");
                        }
                        else
                        {
                            _logger.LogWarning("CreateAuctionProductAsync returned false without throwing an exception");
                            ModelState.AddModelError(string.Empty, "Failed to create product. Please check the logs for details.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    
                    // Provide a user-friendly error message
                    if (!ModelState.Any(m => m.Value.Errors.Count > 0))
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while creating the product. Please check all fields and try again.");
                    }
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

        // POST: Home/CreateBorrowProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBorrowProduct(BorrowProduct borrowProduct, string tagIds, string imageUrls)
        {
            _logger.LogInformation("Creating a new borrow product");
            
            // Debug log all received values
            _logger.LogInformation("Received borrowProduct: Title={Title}, Description={Description}, " +
                "CategoryId={CategoryId}, ConditionId={ConditionId}, " +
                "TimeLimit={TimeLimit}, StartDate={StartDate}, EndDate={EndDate}, " +
                "DailyRate={DailyRate}, IsBorrowed={IsBorrowed}, " +
                "SellerId={SellerId}, tagIds={tagIds}, imageUrls={imageUrlsLength}",
                borrowProduct?.Title,
                borrowProduct?.Description?.Substring(0, Math.Min(20, borrowProduct?.Description?.Length ?? 0)) + "...",
                borrowProduct?.CategoryId,
                borrowProduct?.ConditionId,
                borrowProduct?.TimeLimit,
                borrowProduct?.StartDate,
                borrowProduct?.EndDate,
                borrowProduct?.DailyRate,
                borrowProduct?.IsBorrowed,
                borrowProduct?.SellerId,
                tagIds,
                imageUrls?.Length ?? 0);
                
            // CRITICAL DATA VALIDATION AND FIXING
            // ===================================
            
            // Ensure we have a valid SellerId
            if (borrowProduct.SellerId <= 0)
            {
                _logger.LogWarning("SellerId was invalid or missing, setting to default (1)");
                borrowProduct.SellerId = 1;
            }
            
            // Ensure we have valid Category and Condition
            if (borrowProduct.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category");
                _logger.LogWarning("CategoryId was invalid or missing");
            }
            
            if (borrowProduct.ConditionId <= 0)
            {
                ModelState.AddModelError("ConditionId", "Please select a valid condition");
                _logger.LogWarning("ConditionId was invalid or missing");
            }
            
            // Ensure DailyRate is set
            if (borrowProduct.DailyRate <= 0)
            {
                _logger.LogWarning("DailyRate was invalid or missing, setting to default (1.0)");
                borrowProduct.DailyRate = 1.0;
            }
            
            // Ensure TimeLimit is set
            if (borrowProduct.TimeLimit == default)
            {
                _logger.LogWarning("TimeLimit was not provided, setting to one month from now");
                borrowProduct.TimeLimit = DateTime.Now.AddMonths(1);
            }
            
            // Ensure StartDate is set
            if (borrowProduct.StartDate == null)
            {
                _logger.LogWarning("StartDate was not provided, setting to now");
                borrowProduct.StartDate = DateTime.Now;
            }
            
            // Ensure EndDate is set
            if (borrowProduct.EndDate == null)
            {
                _logger.LogWarning("EndDate was not provided, setting to one month from now");
                borrowProduct.EndDate = DateTime.Now.AddMonths(1);
            }
            
            // Process tags
            var productTags = new List<ProductTag>();
            if (!string.IsNullOrEmpty(tagIds))
            {
                _logger.LogInformation("Processing tags: {TagIds}", tagIds);
                var tagIdList = tagIds.Split(',');
                foreach (var tagId in tagIdList)
                {
                    if (tagId.StartsWith("new_"))
                    {
                        var tagTitle = tagId.Substring(4); // Remove "new_" prefix
                        _logger.LogInformation("Creating new tag: {TagTitle}", tagTitle);
                        var newTag = _productTagService.CreateProductTag(tagTitle);
                        productTags.Add(newTag);
                    }
                    else if (int.TryParse(tagId, out int existingTagId))
                    {
                        var allTags = _productTagService.GetAllProductTags();
                        var tag = allTags.FirstOrDefault(t => t.Id == existingTagId);
                        if (tag != null)
                        {
                            productTags.Add(tag);
                        }
                    }
                }
            }
            
            // Process image URLs
            var productImages = new List<Image>();
            if (!string.IsNullOrEmpty(imageUrls))
            {
                _logger.LogInformation("Processing image URLs: {ImageUrls}", imageUrls);
                productImages = _imageUploadService.ParseImagesString(imageUrls);
                _logger.LogInformation("Parsed {ImageCount} images", productImages.Count);
                borrowProduct.NonMappedImages = productImages.ToList();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to create borrow product: {Title}", borrowProduct.Title);
                    
                    // Create the borrow product using the service
                    var result = false;
                    try 
                    {
                        // Log the complete object being sent
                        _logger.LogInformation("Sending to service: Title={Title}, CategoryId={CategoryId}, ConditionId={ConditionId}, " +
                            "TimeLimit={TimeLimit}, StartDate={StartDate}, EndDate={EndDate}, " +
                            "DailyRate={DailyRate}, SellerId={SellerId}, " +
                            "ImageCount={ImageCount}, TagCount={TagCount}",
                            borrowProduct.Title,
                            borrowProduct.CategoryId,
                            borrowProduct.ConditionId,
                            borrowProduct.TimeLimit,
                            borrowProduct.StartDate,
                            borrowProduct.EndDate,
                            borrowProduct.DailyRate,
                            borrowProduct.SellerId,
                            borrowProduct.NonMappedImages?.Count ?? 0,
                            productTags.Count);
                            
                        // Attempt creation
                        result = await _borrowProductsService.CreateBorrowProductAsync(borrowProduct);
                        _logger.LogInformation("CreateBorrowProductAsync completed with result: {Result}", result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception in CreateBorrowProductAsync: {Message}", ex.Message);
                        if (ex.InnerException != null)
                        {
                            _logger.LogError("Inner exception: {Message}", ex.InnerException.Message);
                            if (ex.InnerException.InnerException != null)
                            {
                                _logger.LogError("Inner inner exception: {Message}", ex.InnerException.InnerException.Message);
                            }
                        }
                        throw;
                    }
                    
                    if (result)
                    {
                        _logger.LogInformation("Borrow product created successfully");
                        return RedirectToAction("Index", "BorrowProducts");
                    }
                    else
                    {
                        _logger.LogWarning("CreateBorrowProductAsync returned false without throwing an exception");
                        ModelState.AddModelError(string.Empty, "Failed to create borrow product. Please check the logs for details.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating borrow product: {Message}", ex.Message);
                    ModelState.AddModelError(string.Empty, $"Failed to create borrow product: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state when creating borrow product: {Errors}", 
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }
            
            // If we get here, something went wrong
            // Reload categories, conditions, and tags for the view
            ViewBag.Categories = _categoryService.GetAllProductCategories();
            ViewBag.Conditions = _conditionService.GetAllProductConditions();
            ViewBag.Tags = _productTagService.GetAllProductTags();
            
            return View("Create", new AuctionProduct()); // Return to Create view with empty auction product
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