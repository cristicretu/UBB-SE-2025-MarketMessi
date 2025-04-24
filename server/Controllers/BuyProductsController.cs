using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;
using server.Models;
using MarketMinds.Repositories.BuyProductsRepository;
using server.Models.DTOs;
using server.Models.DTOs.Mappers;
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
        [ProducesResponseType(typeof(List<BuyProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBuyProducts()
        {
            try
            {
                var products = _buyProductsRepository.GetProducts();
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
                var product = _buyProductsRepository.GetProductByID(id);
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
                Console.WriteLine($"Error deleting buy product ID {id}: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the product.");
            }
        }
    }
}