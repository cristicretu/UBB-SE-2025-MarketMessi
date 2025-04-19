using Microsoft.AspNetCore.Mvc;
using DataAccessLayer; 
using server.Models; 
using MarketMinds.Repositories.BorrowProductsRepository; 

using System.Collections.Generic;
using System;
using System.Net;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class BorrowProductsController : ControllerBase
    {
        private readonly IBorrowProductsRepository _borrowProductsRepository;

        public BorrowProductsController(IBorrowProductsRepository borrowProductsRepository)
        {
            _borrowProductsRepository = borrowProductsRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BorrowProduct>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBorrowProducts()
        {
            try
            {
                var products = _borrowProductsRepository.GetProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all borrow products: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BorrowProduct), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBorrowProductById(int id)
        {
            try
            {
                var product = _borrowProductsRepository.GetProductByID(id);
                if (product == null)
                {
                    return NotFound($"Borrow product with ID {id} not found.");
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting borrow product by ID {id}: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(BorrowProduct), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateBorrowProduct([FromBody] BorrowProduct product)
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
                _borrowProductsRepository.AddProduct(product);
                return CreatedAtAction(nameof(GetBorrowProductById), new { id = product.Id }, product);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating borrow product: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the product.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateBorrowProduct(int id, [FromBody] BorrowProduct product)
        {
            if (product == null || id != product.Id || !ModelState.IsValid)
            {
                return BadRequest("Product data is invalid or ID mismatch.");
            }

            try
            {
                _borrowProductsRepository.UpdateProduct(product);
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
                Console.WriteLine($"Error updating borrow product ID {id}: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while updating the product.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteBorrowProduct(int id)
        {
            try
            {
                var productToDelete = _borrowProductsRepository.GetProductByID(id);
                if (productToDelete == null)
                {
                    return NotFound($"Borrow product with ID {id} not found.");
                }

                _borrowProductsRepository.DeleteProduct(productToDelete);
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
                Console.WriteLine($"Error deleting borrow product ID {id}: {ex}"); 
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the product.");
            }
        }
    }
} 