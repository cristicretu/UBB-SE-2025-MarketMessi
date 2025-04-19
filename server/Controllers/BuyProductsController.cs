using Microsoft.AspNetCore.Mvc;
using MarketMinds.Repositories.BuyProductsRepository;
using server.Models;

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
        public ActionResult<List<Product>> GetAllProducts()
        {
            try
            {
                var products = _buyProductsRepository.GetProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProductById(int id)
        {
            try
            {
                var product = _buyProductsRepository.GetProductByID(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public ActionResult<Product> AddProduct([FromBody] Product product)
        {
            try
            {
                _buyProductsRepository.AddProduct(product);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest("Product ID mismatch");
                }

                _buyProductsRepository.UpdateProduct(product);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                var product = _buyProductsRepository.GetProductByID(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                _buyProductsRepository.DeleteProduct(product);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 