using System;
using MarketMinds.Repositories.ReviewRepository;
using server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        public ReviewController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        [HttpGet("buyer/{buyerId}")]
        [ProducesResponseType(typeof(ObservableCollection<Review>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetReviewsByBuyer(int buyerId)
        {
            try
            {
                var buyer = new User { Id = buyerId };
                var reviews = _reviewRepository.GetAllReviewsByBuyer(buyer);

                // Convert ReviewImages to generic Images for each review
                foreach (var review in reviews)
                {
                    review.LoadGenericImages();
                }

                return Ok(reviews);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reviews by buyer ID: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("seller/{sellerId}")]
        [ProducesResponseType(typeof(ObservableCollection<Review>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetReviewsBySeller(int sellerId)
        {
            try
            {
                var seller = new User { Id = sellerId };
                var reviews = _reviewRepository.GetAllReviewsBySeller(seller);

                // Convert ReviewImages to generic Images for each review
                foreach (var review in reviews)
                {
                    review.LoadGenericImages();
                }

                return Ok(reviews);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reviews by seller ID: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Review), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateReview([FromBody] Review review)
        {
            if (review == null)
            {
                return BadRequest("Review cannot be null.");
            }
            try
            {
                // Set ID to 0 to ensure Entity Framework knows it's a new entity
                review.Id = 0;

                // Create the review first to get an ID
                _reviewRepository.CreateReview(review);

                // Return the created review with its new ID
                return CreatedAtAction(nameof(GetReviewsByBuyer), new { buyerId = review.BuyerId }, review);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating review: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(Review), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult EditReview([FromBody] Review review)
        {
            if (review == null)
            {
                return BadRequest("Review cannot be null.");
            }

            try
            {
                // Sync images with ReviewImages for DB storage
                review.SyncImagesBeforeSave();

                _reviewRepository.EditReview(review, review.Rating, review.Description);
                return Ok(review);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing review: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteReview([FromBody] Review review)
        {
            try
            {
                if (review == null)
                {
                    return BadRequest("Review cannot be null.");
                }
                _reviewRepository.DeleteReview(review);
                return NoContent();
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting review: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }
    }
}