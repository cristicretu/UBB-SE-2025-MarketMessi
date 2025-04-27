using System;
using System.Net;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;
using server.Models;
using server.Models.DTOs;
using server.Models.DTOs.Mappers;
using MarketMinds.Repositories.AuctionProductsRepository;

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
                if (ModelState.ErrorCount > 0 && incomingImages.Count > 0) 
                {
                    // Remove potential errors like "Images[0].Product" if they exist
                    var imageKeys = ModelState.Keys.Where(k => k.StartsWith("Images[")).ToList();
                    foreach (var key in imageKeys)
                    {
                        ModelState.Remove(key);
                    }
                }
                // Re-check if model state is valid after potentially removing image errors
                if (!ModelState.IsValid) 
                {
                    Console.WriteLine($"Model State is Invalid (after image handling): {System.Text.Json.JsonSerializer.Serialize(ModelState)}");
                    return BadRequest(ModelState);
                }
            }
            if (product.Id != 0) 
            {
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
                    try 
                    {
                        var savedProduct = _auctionProductsRepository.GetProductByID(product.Id);
                        Console.WriteLine($"Retrieved product has {savedProduct.Images.Count} image(s) after save");
                    }
                    catch (Exception ex) 
                    {
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

        [HttpPost("{id}/bids")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult PlaceBid(int id, [FromBody] CreateBidDTO bidDTO)
        {
            if (bidDTO == null || id != bidDTO.ProductId)
            {
                return BadRequest("Invalid bid data or ID mismatch.");
            }
            try 
            {
                var product = _auctionProductsRepository.GetProductByID(id);
                // Handle refund for previous bidder if any
                if (product.Bids.Any())
                {
                    var previousBid = product.Bids.OrderByDescending(b => b.Timestamp).FirstOrDefault();
                    if (previousBid != null)
                    {
                        var previousBidder = previousBid.Bidder;
                        if (previousBidder != null)
                        {
                            // Refund the previous bidder's balance
                            Console.WriteLine($"Refunding previous bidder (ID: {previousBidder.Id}) with amount: {previousBid.Price}");
                            previousBidder.Balance += (double)previousBid.Price;
                            // _userRepository.UpdateUser(previousBidder);
                        }
                    }
                }
                // Create the bid
                var bid = new Bid
                {
                    BidderId = bidDTO.BidderId,
                    ProductId = id,
                    Price = bidDTO.Amount,
                    Timestamp = bidDTO.Timestamp
                };
                // Add bid to product and update current price
                product.Bids.Add(bid);
                product.CurrentPrice = bidDTO.Amount;
                _auctionProductsRepository.UpdateProduct(product);
                return Ok(new { Success = true, Message = $"Bid of ${bidDTO.Amount} placed successfully." });
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error placing bid on auction product ID {id}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while placing the bid.");
            }
        }
    }
} 