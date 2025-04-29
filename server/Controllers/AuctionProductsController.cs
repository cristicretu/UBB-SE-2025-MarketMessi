using System.Net;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Models.DTOs.Mappers;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionProductsController : ControllerBase
    {
        private readonly IAuctionProductsRepository auctionProductsRepository;
        private readonly static int NULL_PRODUCT_ID = 0;

        public AuctionProductsController(IAuctionProductsRepository auctionProductsRepository)
        {
            this.auctionProductsRepository = auctionProductsRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AuctionProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAuctionProducts()
        {
            try
            {
                var products = auctionProductsRepository.GetProducts();
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
                var product = auctionProductsRepository.GetProductByID(id);
                var auctionProductDTO = AuctionProductMapper.ToDTO(product);
                return Ok(auctionProductDTO);
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
            if (product.StartTime == default(DateTime))
            {
                product.StartTime = DateTime.Now;
                Console.WriteLine($"Server set default StartTime: {product.StartTime}");
            }
            
            if (product.EndTime == default(DateTime))
            {
                product.EndTime = DateTime.Now.AddDays(7);
                Console.WriteLine($"Server set default EndTime: {product.EndTime}");
            }
            else 
            {
                if (product.EndTime.Year < 2000)
                {
                    product.EndTime = DateTime.Now.AddDays(7);
                }
                else
                {
                }
            }
            
            if (product.StartPrice <= 0)
            {
                if (product.CurrentPrice > 0)
                {
                    product.StartPrice = product.CurrentPrice;
                }
                else
                {
                    product.StartPrice = 1.0; // Default minimum price
                    product.CurrentPrice = 1.0;
                }
            }
            
            var incomingImages = product.Images?.ToList() ?? new List<ProductImage>();
            product.Images = new List<ProductImage>();
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
            if (product.Id != NULL_PRODUCT_ID)
            {
                return BadRequest("Product ID should not be provided when creating a new product.");
            }
            try
            {
                // Add the product (without images linked yet)
                auctionProductsRepository.AddProduct(product);
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
                    auctionProductsRepository.UpdateProduct(product);

                    // Verify images were saved by retrieving the product again
                    try
                    {
                        var savedProduct = auctionProductsRepository.GetProductByID(product.Id);
                        Console.WriteLine($"Retrieved product has {savedProduct.Images.Count} image(s) after save");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error verifying images: {ex.Message}");
                    }
                }
                var auctionProductDTO = AuctionProductMapper.ToDTO(product);
                return CreatedAtAction(nameof(GetAuctionProductById), new { id = product.Id }, auctionProductDTO);
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
                auctionProductsRepository.UpdateProduct(product);
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
                var productToDelete = auctionProductsRepository.GetProductByID(id);
                auctionProductsRepository.DeleteProduct(productToDelete);
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
                var product = auctionProductsRepository.GetProductByID(id);
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
                auctionProductsRepository.UpdateProduct(product);
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