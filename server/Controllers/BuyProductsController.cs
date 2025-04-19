using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;
using server.Models;
using MarketMinds.Repositories.BuyProductsRepository;
using System.Collections.Generic;
using System;
using System.Net;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuyProductsController : ControllerBase
    {
        private readonly IBuyProductsRepository _buyProductsRepository;

        public BuyProductsController(IBuyProductsRepository buyProductsRepository)
        {
            _buyProductsRepository = buyProductsRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Product>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetProducts()
        {
            try
            {
                var products = _buyProductsRepository.GetProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all products: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetProductById(int id)
        {
            try
            {
                var product = _buyProductsRepository.GetProductByID(id);
                return Ok(product);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product by ID {id}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            if (product == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (product.Id != 0)
            {
                return BadRequest("Product ID should not be provided when creating a new product.");
            }

            try
            {
                _buyProductsRepository.AddProduct(product);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the product.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            if (product == null || id != product.Id || !ModelState.IsValid)
            {
                return BadRequest("Product data is invalid or ID mismatch.");
            }

            try
            {
                _buyProductsRepository.UpdateProduct(product);
                return NoContent();
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product ID {id}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while updating the product.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                var productToDelete = _buyProductsRepository.GetProductByID(id);
                _buyProductsRepository.DeleteProduct(productToDelete);
                return NoContent();
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product ID {id}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the product.");
            }
        }
    }
}