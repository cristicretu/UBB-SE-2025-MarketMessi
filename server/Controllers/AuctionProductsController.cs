using Microsoft.AspNetCore.Mvc;
using DataAccessLayer; // Corrected namespace
using server.Models; // Added using statement
using MarketMinds.Repositories.AuctionProductsRepository; // Corrected namespace
// using DomainLayer.Domain; // Removed incorrect using
using System.Collections.Generic;
using System;
using System.Net;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route: /api/auctionproducts
    public class AuctionProductsController : ControllerBase
    {
        private readonly IAuctionProductsRepository _auctionProductsRepository;

        public AuctionProductsController(IAuctionProductsRepository auctionProductsRepository)
        {
            _auctionProductsRepository = auctionProductsRepository;
        }

        // GET: api/auctionproducts
        [HttpGet]
        [ProducesResponseType(typeof(List<Product>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAuctionProducts()
        {
            try
            {
                var products = _auctionProductsRepository.GetProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error getting all auction products: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        // GET: api/auctionproducts/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuctionProduct), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAuctionProductById(int id)
        {
             try
            {
                var product = _auctionProductsRepository.GetProductByID(id);
                // GetProductByID now throws KeyNotFoundException if not found
                return Ok(product);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                 // Log exception
                Console.WriteLine($"Error getting auction product by ID {id}: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        // POST: api/auctionproducts
        [HttpPost]
        [ProducesResponseType(typeof(AuctionProduct), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateAuctionProduct([FromBody] AuctionProduct product)
        {
            // Basic validation (more robust validation using FluentValidation or DataAnnotations is recommended)
             if (product == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

             // Ensure ID is not set by client, or handle appropriately
             if (product.Id != 0) {
                 // Or ignore the client-provided ID
                 return BadRequest("Product ID should not be provided when creating a new product.");
             }

            try
            {
                _auctionProductsRepository.AddProduct(product);
                 // Return 201 Created with the location of the new resource and the created object
                 // The object now has the ID assigned by the database
                return CreatedAtAction(nameof(GetAuctionProductById), new { id = product.Id }, product);
            }
             catch (ArgumentException aex)
            {
                 // If AddProduct validates type incorrectly (shouldn't happen if parameter is AuctionProduct)
                 return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error creating auction product: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the product.");
            }
        }

        // PUT: api/auctionproducts/{id}
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateAuctionProduct(int id, [FromBody] AuctionProduct product)
        {
            if (product == null || id != product.Id || !ModelState.IsValid) // Check if ID in route matches ID in body
            {
                return BadRequest("Product data is invalid or ID mismatch.");
            }

            try
            {
                _auctionProductsRepository.UpdateProduct(product);
                 // UpdateProduct throws KeyNotFoundException if not found
                return NoContent(); // Standard response for successful PUT
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
                // Log exception
                Console.WriteLine($"Error updating auction product ID {id}: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while updating the product.");
            }
        }

        // DELETE: api/auctionproducts/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteAuctionProduct(int id)
        {
             try
            {
                 // Need a way to pass the ID to DeleteProduct, repository currently expects the full object.
                 // Option 1: Modify repository DeleteProduct(int id)
                 // Option 2: Get the product first, then delete (less efficient)
                 // Let's assume we modify the repo (need to update interface too)
                 // For now, let's retrieve then delete (demonstrates pattern but less ideal)
                
                 var productToDelete = _auctionProductsRepository.GetProductByID(id);
                 // If GetProductByID didn't throw KeyNotFoundException, the product exists.
                 _auctionProductsRepository.DeleteProduct(productToDelete);

                return NoContent(); // Standard response for successful DELETE
            }
             catch (KeyNotFoundException knfex)
            {
                 // Can be thrown by GetProductByID or DeleteProduct
                return NotFound(knfex.Message);
            }
             catch (ArgumentException aex)
            {
                 // If DeleteProduct validates type incorrectly
                 return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                // Log exception
                 Console.WriteLine($"Error deleting auction product ID {id}: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the product.");
            }
        }
    }
} 