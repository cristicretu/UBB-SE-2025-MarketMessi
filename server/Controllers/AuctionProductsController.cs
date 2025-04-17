using Microsoft.AspNetCore.Mvc;
using DataAccessLayer; 
using server.Models; 
using MarketMinds.Repositories.AuctionProductsRepository; 

using System.Collections.Generic;
using System;
using System.Net;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class AuctionProductsController : ControllerBase
    {
        private readonly IAuctionProductsRepository _auctionProductsRepository;

        public AuctionProductsController(IAuctionProductsRepository auctionProductsRepository)
        {
            _auctionProductsRepository = auctionProductsRepository;
        }

        
        [HttpGet]
        [ProducesResponseType(typeof(List<AuctionProduct>), (int)HttpStatusCode.OK)]
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
                
                Console.WriteLine($"Error getting all auction products: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuctionProduct), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAuctionProductById(int id)
        {
             try
            {
                var product = _auctionProductsRepository.GetProductByID(id);
                
                return Ok(product);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                 
                Console.WriteLine($"Error getting auction product by ID {id}: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        
        [HttpPost]
        [ProducesResponseType(typeof(AuctionProduct), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateAuctionProduct([FromBody] AuctionProduct product)
        {
            
             if (product == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

             
             if (product.Id != 0) {
                 
                 return BadRequest("Product ID should not be provided when creating a new product.");
             }

            try
            {
                _auctionProductsRepository.AddProduct(product);
                 
                 
                return CreatedAtAction(nameof(GetAuctionProductById), new { id = product.Id }, product);
            }
             catch (ArgumentException aex)
            {
                 
                 return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error creating auction product: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the product.");
            }
        }

        
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateAuctionProduct(int id, [FromBody] AuctionProduct product)
        {
            if (product == null || id != product.Id || !ModelState.IsValid) 
            {
                return BadRequest("Product data is invalid or ID mismatch.");
            }

            try
            {
                _auctionProductsRepository.UpdateProduct(product);
                 
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
                
                Console.WriteLine($"Error updating auction product ID {id}: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while updating the product.");
            }
        }

        
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteAuctionProduct(int id)
        {
             try
            {
                 
                 
                 
                 
                 
                
                 var productToDelete = _auctionProductsRepository.GetProductByID(id);
                 
                 _auctionProductsRepository.DeleteProduct(productToDelete);

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
                
                 Console.WriteLine($"Error deleting auction product ID {id}: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the product.");
            }
        }
    }
} 