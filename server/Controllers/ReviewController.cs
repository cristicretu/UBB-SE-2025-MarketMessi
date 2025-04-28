using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MarketMinds.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Models.DTOs.Mappers;
using MarketMinds.Shared.IRepository;
using MarketMinds.Repositories.ReviewRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository reviewRepository;
        private readonly static int DEFAULT_REVIEW_ID = 0;
        public ReviewController(IReviewRepository reviewRepository)
        {
            this.reviewRepository = reviewRepository;
        }

        [HttpGet("buyer/{buyerId}")]
        [ProducesResponseType(typeof(List<ReviewDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetReviewsByBuyer(int buyerId)
        {
            try
            {
                var buyer = new User { Id = buyerId };
                var reviews = reviewRepository.GetAllReviewsByBuyer(buyer);

                // Convert ReviewImages to generic Images for each review
                foreach (var review in reviews)
                {
                    review.LoadGenericImages();
                }

                // Convert to DTOs to avoid circular references
                var reviewDtos = reviews.Select(review => ReviewMapper.ToDto(review)).ToList();
                return Ok(reviewDtos);
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
        [ProducesResponseType(typeof(List<ReviewDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetReviewsBySeller(int sellerId)
        {
            try
            {
                var seller = new User { Id = sellerId };
                var reviews = reviewRepository.GetAllReviewsBySeller(seller);

                // Convert ReviewImages to generic Images for each review
                foreach (var review in reviews)
                {
                    review.LoadGenericImages();
                }

                // Convert to DTOs to avoid circular references
                var reviewDtos = reviews.Select(review => ReviewMapper.ToDto(review)).ToList();
                return Ok(reviewDtos);
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
        [ProducesResponseType(typeof(ReviewDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateReview([FromBody] ReviewDTO reviewDto)
        {
            if (reviewDto == null)
            {
                return BadRequest("Review cannot be null.");
            }
            try
            {
                // Convert DTO to model
                var review = ReviewMapper.ToModel(reviewDto);

                // Set ID to 0 to ensure Entity Framework knows it's a new entity
                review.Id = DEFAULT_REVIEW_ID;

                // Create the review
                reviewRepository.CreateReview(review);

                // Convert back to DTO for response
                var createdReviewDto = ReviewMapper.ToDto(review);

                // Return the created review with its new ID
                return CreatedAtAction(nameof(GetReviewsByBuyer), new { buyerId = review.BuyerId }, createdReviewDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating review: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ReviewDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult EditReview([FromBody] ReviewDTO reviewDto)
        {
            if (reviewDto == null)
            {
                return BadRequest("Review cannot be null.");
            }

            try
            {
                // Convert DTO to model
                var review = ReviewMapper.ToModel(reviewDto);

                // Sync images with ReviewImages for DB storage
                review.SyncImagesBeforeSave();

                reviewRepository.EditReview(review, review.Rating, review.Description);

                // Convert back to DTO for response
                var updatedReviewDto = ReviewMapper.ToDto(review);
                return Ok(updatedReviewDto);
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
        public IActionResult DeleteReview([FromBody] ReviewDTO reviewDto)
        {
            try
            {
                if (reviewDto == null)
                {
                    return BadRequest("Review cannot be null.");
                }

                // Convert DTO to model
                var review = ReviewMapper.ToModel(reviewDto);

                reviewRepository.DeleteReview(review);
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