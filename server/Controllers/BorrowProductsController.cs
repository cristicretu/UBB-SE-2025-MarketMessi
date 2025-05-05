using System.Net;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.IRepository;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowProductsController : ControllerBase
    {
        private readonly IBorrowProductsRepository borrowProductsRepository;
        private readonly static int NULL_PRODUCT_ID = 0;

        public BorrowProductsController(IBorrowProductsRepository borrowProductsRepository)
        {
            this.borrowProductsRepository = borrowProductsRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BorrowProduct>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBorrowProducts()
        {
            try
            {
                var products = borrowProductsRepository.GetProducts();

                // If products is empty, it could be due to an error
                if (!products.Any())
                {
                    Console.WriteLine("Warning: No borrow products returned. This could be due to database connectivity issues or schema mismatches.");
                }

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
                var product = borrowProductsRepository.GetProductByID(id);
                return Ok(product);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
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
        [Consumes("application/json")]
        public IActionResult CreateBorrowProduct([FromBody] CreateBorrowProductDTO productDTO)
        {
            // Verify correct method is being called
            Console.WriteLine("=== IMPORTANT VERIFICATION ===");
            Console.WriteLine("THIS IS THE DTO VERSION OF CREATE BORROW PRODUCT");
            Console.WriteLine("=== VERIFICATION COMPLETE ===");

            // Diagnostic logging
            Console.WriteLine("==== BORROW PRODUCT CREATION DIAGNOSTICS ====");
            Console.WriteLine($"Received object type: {productDTO?.GetType().FullName ?? "null"}");
            Console.WriteLine($"Model binding state: {(ModelState.IsValid ? "Valid" : "Invalid")}");
            Console.WriteLine("ModelState errors:");
            foreach (var error in ModelState)
            {
                Console.WriteLine($"- {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
            Console.WriteLine($"Raw request content: {Request.Body}");
            Console.WriteLine("Controller Assembly: " + this.GetType().Assembly.FullName);
            Console.WriteLine("DTO Assembly: " + typeof(CreateBorrowProductDTO).Assembly.FullName);
            Console.WriteLine("====================================");

            Console.WriteLine($"Received borrow product DTO in API: {System.Text.Json.JsonSerializer.Serialize(productDTO)}");

            // Explicitly ignore navigation property validation errors
            ModelState.Remove("Seller");
            ModelState.Remove("Category");
            ModelState.Remove("Condition");
            ModelState.Remove("Images");

            // Check keys that might contain "Product" validation errors
            foreach (var key in ModelState.Keys.Where(k => k.Contains("Product")).ToList())
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine($"Model State is Invalid: {System.Text.Json.JsonSerializer.Serialize(ModelState)}");
                return BadRequest(ModelState);
            }

            try
            {
                // Create a BorrowProduct from the DTO
                var product = new BorrowProduct
                {
                    Title = productDTO.Title,
                    Description = productDTO.Description,
                    SellerId = productDTO.SellerId,
                    CategoryId = productDTO.CategoryId ?? NULL_PRODUCT_ID,
                    ConditionId = productDTO.ConditionId ?? NULL_PRODUCT_ID,
                    TimeLimit = productDTO.TimeLimit,
                    StartDate = productDTO.StartDate,
                    EndDate = productDTO.EndDate,
                    DailyRate = productDTO.DailyRate,
                    IsBorrowed = productDTO.IsBorrowed
                };
                if (product.StartDate == default(DateTime))
                {
                    product.StartDate = DateTime.Now;
                    Console.WriteLine($"Server set default StartDate: {product.StartDate}");
                }
                else
                {
                    Console.WriteLine($"Server preserved client StartDate: {product.StartDate}");
                }

                if (product.EndDate == default(DateTime))
                {
                    product.EndDate = DateTime.Now.AddDays(7);
                    Console.WriteLine($"Server set default EndDate: {product.EndDate}");
                }
                else
                {
                    Console.WriteLine($"Server preserved client EndDate: {product.EndDate}");
                }

                if (product.TimeLimit == default(DateTime))
                {
                    product.TimeLimit = DateTime.Now.AddDays(7);
                    Console.WriteLine($"Server set default TimeLimit: {product.TimeLimit}");
                }
                else
                {
                    Console.WriteLine($"Server preserved client TimeLimit: {product.TimeLimit}");
                }

                Console.WriteLine($"Mapped DTO to product: SellerId={product.SellerId}, CategoryId={product.CategoryId}, ConditionId={product.ConditionId}");

                // Add the product without images
                borrowProductsRepository.AddProduct(product);
                Console.WriteLine($"Added product with ID: {product.Id}");

                // Process images if any
                if (productDTO.Images != null && productDTO.Images.Any())
                {
                    foreach (var imgDTO in productDTO.Images)
                    {
                        Console.WriteLine($"Adding image with URL: {imgDTO.Url}");

                        var img = new BorrowProductImage
                        {
                            Url = imgDTO.Url,
                            ProductId = product.Id
                        };

                        borrowProductsRepository.AddImageToProduct(product.Id, img);
                    }

                    Console.WriteLine($"Images added to repository.");

                    // Verify images were saved
                    try
                    {
                        var savedProduct = borrowProductsRepository.GetProductByID(product.Id);
                        Console.WriteLine($"Retrieved product has {savedProduct.Images.Count} image(s) after save");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error verifying images: {ex.Message}");
                    }
                }

                return CreatedAtAction(nameof(GetBorrowProductById), new { id = product.Id }, product);
            }
            catch (ArgumentException aex)
            {
                Console.WriteLine($"Argument exception: {aex.Message}");
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating borrow product: {ex}");
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
        public IActionResult UpdateBorrowProduct(int id, [FromBody] BorrowProduct product)
        {
            if (product == null || id != product.Id || !ModelState.IsValid)
            {
                return BadRequest("Product data is invalid or ID mismatch.");
            }

            try
            {
                borrowProductsRepository.UpdateProduct(product);
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
                var productToDelete = borrowProductsRepository.GetProductByID(id);
                borrowProductsRepository.DeleteProduct(productToDelete);
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