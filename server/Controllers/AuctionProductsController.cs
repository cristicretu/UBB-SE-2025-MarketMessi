using Microsoft.AspNetCore.Mvc;
using DataAccessLayer; 
using server.Models; 
using MarketMinds.Repositories.AuctionProductsRepository; 
using server.Models.DTOs;
using server.Models.DTOs.Mappers;

using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;

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
        [ProducesResponseType(typeof(List<AuctionProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAuctionProducts()
        {
            try
            {
                var products = _auctionProductsRepository.GetProducts();
                var dtos = AuctionProductMapper.ToDTOList(products);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error getting all auction products: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuctionProductDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAuctionProductById(int id)
        {
             try
            {
                var product = _auctionProductsRepository.GetProductByID(id);
                var dto = AuctionProductMapper.ToDTO(product);
                return Ok(dto);
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
        [ProducesResponseType(typeof(AuctionProductDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateAuctionProduct([FromBody] AuctionProduct product)
        {

            var incomingImages = product.Images?.ToList() ?? new List<ProductImage>();
            product.Images = new List<ProductImage>(); 

            // Validate the main product (without images for now)
            if (product == null || !ModelState.IsValid)
            {
                // Manually remove image validation errors if they occurred before clearing
                ModelState.Remove("Images"); 
                if (ModelState.ErrorCount > 0 && incomingImages.Count > 0) {
                    // Remove potential errors like "Images[0].Product" if they exist
                    var imageKeys = ModelState.Keys.Where(k => k.StartsWith("Images[")).ToList();
                    foreach(var key in imageKeys) {
                        ModelState.Remove(key);
                    }
                }
                
                // Re-check if model state is valid after potentially removing image errors
                if (!ModelState.IsValid) {
                    Console.WriteLine($"Model State is Invalid (after image handling): {System.Text.Json.JsonSerializer.Serialize(ModelState)}");
                    return BadRequest(ModelState);
                }
            }

            if (product.Id != 0) {
                return BadRequest("Product ID should not be provided when creating a new product.");
            }

            try
            {
                // Add the product (without images linked yet)
                _auctionProductsRepository.AddProduct(product); 
                // Now 'product' has its Id assigned by the database

                // Link and add images if any were provided
                if (incomingImages.Any())
                {
                    
                    foreach (var img in incomingImages)
                    {
                        img.ProductId = product.Id; // Set the foreign key
                        // Assuming ProductImage doesn't have other required fields from client
                        // Add the now-linked image back to the product's collection
                        product.Images.Add(img);
                    }
                    
                    // Update the product to save the linked images
                    _auctionProductsRepository.UpdateProduct(product);
                    
                    // Verify images were saved by retrieving the product again
                    try {
                        var savedProduct = _auctionProductsRepository.GetProductByID(product.Id);
                        Console.WriteLine($"Retrieved product has {savedProduct.Images.Count} image(s) after save");
                    } catch(Exception ex) {
                        Console.WriteLine($"Error verifying images: {ex.Message}");
                    }
                }

                var dto = AuctionProductMapper.ToDTO(product);
                return CreatedAtAction(nameof(GetAuctionProductById), new { id = product.Id }, dto);
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