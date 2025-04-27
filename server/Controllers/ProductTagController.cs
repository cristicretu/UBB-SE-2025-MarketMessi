using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using MarketMinds.Repositories.ProductTagRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductTagController : ControllerBase
    {
        private readonly IProductTagRepository productTagRepository;

        public ProductTagController(IProductTagRepository productTagRepository)
        {
            this.productTagRepository = productTagRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProductTag>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAllProductTags()
        {
            try
            {
                var allTags = productTagRepository.GetAllProductTags();
                return Ok(allTags);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error getting all product tags: {exception}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductTag), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateProductTag([FromBody] ProductTagRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.DisplayTitle) || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newTag = productTagRepository.CreateProductTag(request.DisplayTitle);

                return CreatedAtAction(
                    nameof(GetAllProductTags),
                    new { id = newTag.Id },
                    newTag);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error creating product tag: {exception}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the tag.");
            }
        }

        [HttpDelete("{title}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteProductTag(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Tag title is required.");
            }

            try
            {
                productTagRepository.DeleteProductTag(title);
                return NoContent();
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error deleting product tag '{title}': {exception}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the tag.");
            }
        }
    }

    public class ProductTagRequest
    {
        public string DisplayTitle { get; set; } = null!;
    }
}