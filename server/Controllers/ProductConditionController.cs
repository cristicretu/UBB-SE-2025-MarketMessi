using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using MarketMinds.Repositories.ProductConditionRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductConditionController : ControllerBase
    {
        private readonly IProductConditionRepository productConditionRepository;

        public ProductConditionController(IProductConditionRepository productConditionRepository)
        {
            this.productConditionRepository = productConditionRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Condition>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAllProductConditions()
        {
            try
            {
                var allConditions = productConditionRepository.GetAllProductConditions();
                return Ok(allConditions);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error getting all product conditions: {exception}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Condition), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateProductCondition([FromBody] ProductConditionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.DisplayTitle) || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newCondition = productConditionRepository.CreateProductCondition(
                    request.DisplayTitle,
                    request.Description);

                return CreatedAtAction(
                    nameof(GetAllProductConditions),
                    new { id = newCondition.Id },
                    newCondition);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error creating product condition: {exception}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the condition.");
            }
        }

        [HttpDelete("{title}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteProductCondition(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Condition title is required.");
            }

            try
            {
                productConditionRepository.DeleteProductCondition(title);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                // If the condition doesn't exist, return success anyway (idempotent delete)
                return NoContent();
            }
            catch (Exception exception)
            {
                // Look for foreign key constraint errors
                if (exception.ToString().Contains("REFERENCE constraint") ||
                    exception.ToString().Contains("FK_BorrowProducts_ProductConditions"))
                {
                    return BadRequest($"Cannot delete condition '{title}' because it is being used by one or more products. Update or delete those products first.");
                }

                Console.WriteLine($"Error deleting product condition '{title}': {exception}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the condition.");
            }
        }
    }

    public class ProductConditionRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}