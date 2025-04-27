using System;
using System.Net;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;
using Server.Models;
using Server.Models.DTOs;
using Server.Models.DTOs.Mappers;
using MarketMinds.Repositories.BuyProductsRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuyProductsController : ControllerBase
    {
        private readonly IBuyProductsRepository buyProductsRepository;

        public BuyProductsController(IBuyProductsRepository buyProductsRepository)
        {
            this.buyProductsRepository = buyProductsRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BuyProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBuyProducts()
        {
            try
            {
                var products = buyProductsRepository.GetProducts();
                var dtos = BuyProductMapper.ToDTOList(products);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all buy products: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BuyProductDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBuyProductById(int id)
        {
            try
            {
                var product = buyProductsRepository.GetProductByID(id);
                var dto = BuyProductMapper.ToDTO(product);
                return Ok(dto);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting buy product by ID {id}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(BuyProductDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateBuyProduct([FromBody] BuyProduct product)
        {
            Console.WriteLine($"Received buy product in API: {JsonSerializer.Serialize(product)}");

            // Store incoming images and clear them before validation
            var incomingImages = product.Images?.ToList() ?? new List<BuyProductImage>();
            product.Images = new List<BuyProductImage>();

            // Log the ModelState before any manipulation
            Console.WriteLine($"Initial ModelState: {JsonSerializer.Serialize(ModelState)}");

            if (product == null || !ModelState.IsValid)
            {
                // Remove the top-level image validation error key
                ModelState.Remove("Images");

                if (ModelState.ErrorCount > 0 && incomingImages.Count > 0)
                {
                    var imageKeys = ModelState.Keys.Where(k => k.StartsWith("Images[")).ToList();
                    foreach (var key in imageKeys)
                    {
                        ModelState.Remove(key);
                    }
                }

                if (!ModelState.IsValid)
                {
                    Console.WriteLine($"Model state still invalid after filtering: {JsonSerializer.Serialize(ModelState)}");
                    return BadRequest(ModelState);
                }
            }

            if (product.Id != 0)
            {
                return BadRequest("Product ID should not be provided when creating a new product.");
            }

            try
            {
                // First save the product without images
                buyProductsRepository.AddProduct(product);

                // Now handle the images
                if (incomingImages.Any())
                {
                    Console.WriteLine($"Processing {incomingImages.Count} images for product ID {product.Id}");

                    foreach (var img in incomingImages)
                    {
                        Console.WriteLine($"Adding image with URL: {img.Url}");
                        img.ProductId = product.Id;
                        // Don't add to in-memory product.Images collection, but directly to the context
                        buyProductsRepository.AddImageToProduct(product.Id, img);
                    }

                    Console.WriteLine($"Images added to repository.");

                    // Verify images were saved by retrieving the product again
                    try
                    {
                        var savedProduct = buyProductsRepository.GetProductByID(product.Id);
                        Console.WriteLine($"Retrieved product has {savedProduct.Images.Count} image(s) after save");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error verifying images: {ex.Message}");
                    }
                }

                var dto = BuyProductMapper.ToDTO(product);
                return CreatedAtAction(nameof(GetBuyProductById), new { id = product.Id }, dto);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating buy product: {ex}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the product.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateBuyProduct(int id, [FromBody] BuyProduct product)
        {
            if (product == null || id != product.Id || !ModelState.IsValid)
            {
                return BadRequest("Product data is invalid or ID mismatch.");
            }

            try
            {
                buyProductsRepository.UpdateProduct(product);
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
                Console.WriteLine($"Error updating buy product ID {id}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while updating the product.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteBuyProduct(int id)
        {
            try
            {
                var productToDelete = buyProductsRepository.GetProductByID(id);
                buyProductsRepository.DeleteProduct(productToDelete);
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
                Console.WriteLine($"Error deleting buy product ID {id}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the product.");
            }
        }
    }
}