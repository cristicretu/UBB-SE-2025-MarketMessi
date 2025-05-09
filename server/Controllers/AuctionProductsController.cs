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
            catch (Exception exception)
            {
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
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(AuctionProductDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateAuctionProduct([FromBody] AuctionProduct product)
        {
            if (product == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (product.Id != NULL_PRODUCT_ID)
            {
                return BadRequest("Product ID should not be provided when creating a new product.");
            }
            
            try
            {
                var incomingImages = product.Images?.ToList() ?? new List<ProductImage>();
                product.Images = new List<ProductImage>();
                
                auctionProductsRepository.AddProduct(product);
                
                if (incomingImages.Any())
                {
                    foreach (var image in incomingImages)
                    {
                        image.ProductId = product.Id;
                        product.Images.Add(image);
                    }

                    auctionProductsRepository.UpdateProduct(product);
                }
                
                var auctionProductDTO = AuctionProductMapper.ToDTO(product);
                return CreatedAtAction(nameof(GetAuctionProductById), new { id = product.Id }, auctionProductDTO);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception exception)
            {
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
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception ex)
            {
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
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception exception)
            {
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
                
                var bid = new Bid
                {
                    BidderId = bidDTO.BidderId,
                    ProductId = id,
                    Price = bidDTO.Amount,
                    Timestamp = bidDTO.Timestamp
                };
                
                product.Bids.Add(bid);
                product.CurrentPrice = bidDTO.Amount;
                auctionProductsRepository.UpdateProduct(product);
                
                return Ok(new { Success = true, Message = $"Bid of ${bidDTO.Amount} placed successfully." });
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while placing the bid.");
            }
        }
    }
}