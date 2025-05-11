using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ProductCategoryService;
using MarketMinds.Shared.Services.ProductConditionService;
using System.Collections.Generic;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IProductCategoryService _categoryService;
        private readonly IProductConditionService _conditionService;

        public AdminController(IProductCategoryService categoryService, IProductConditionService conditionService)
        {
            _categoryService = categoryService;
            _conditionService = conditionService;
        }

        public IActionResult Index()
        {
            var categories = _categoryService.GetAllProductCategories();
            var conditions = _conditionService.GetAllProductConditions();
            
            ViewBag.Categories = categories;
            ViewBag.Conditions = conditions;
            
            return View();
        }

        [HttpPost]
        public IActionResult AddCategory(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Category name cannot be empty";
                return RedirectToAction("Index");
            }

            try
            {
                _categoryService.CreateProductCategory(name, description);
                TempData["Success"] = "Category added successfully";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error adding category: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddCondition(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Condition name cannot be empty";
                return RedirectToAction("Index");
            }

            try
            {
                _conditionService.CreateProductCondition(name, description);
                TempData["Success"] = "Condition added successfully";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error adding condition: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteCategory(string name)
        {
            try
            {
                _categoryService.DeleteProductCategory(name);
                TempData["Success"] = "Category deleted successfully";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error deleting category: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteCondition(string name)
        {
            try
            {
                _conditionService.DeleteProductCondition(name);
                TempData["Success"] = "Condition deleted successfully";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error deleting condition: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
} 