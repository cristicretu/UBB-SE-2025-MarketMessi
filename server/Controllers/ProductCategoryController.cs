using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;
using server.Models;
using MarketMinds.Repositories.ProductCategoryRepository;
using System.Collections.Generic;
using System;
using System.Net;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public ProductCategoryController(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Category>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAllProductCategories()
        {
            try
            {
                var categories = _productCategoryRepository.GetAllProductCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all product categories: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateProductCategory([FromBody] ProductCategoryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.DisplayTitle) || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newCategory = _productCategoryRepository.CreateProductCategory(
                    request.DisplayTitle,
                    request.Description
                );
                
                return CreatedAtAction(
                    nameof(GetAllProductCategories), 
                    new { id = newCategory.Id }, 
                    newCategory
                );
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product category: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the category.");
            }
        }

        [HttpDelete("{title}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteProductCategory(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Category title is required.");
            }

            try
            {
                _productCategoryRepository.DeleteProductCategory(title);
                return NoContent();
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product category '{title}': {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the category.");
            }
        }
    }

    // Request model for creating product categories
    public class ProductCategoryRequest
    {
        public string DisplayTitle { get; set; }
        public string Description { get; set; }
    }
}