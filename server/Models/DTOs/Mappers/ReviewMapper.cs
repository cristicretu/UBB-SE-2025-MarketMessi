using System.Linq;
using Server.Models;
using Server.Models.DTOs;

namespace Server.Models.DTOs.Mappers
{
    public static class ReviewMapper
    {
        public static ReviewDTO ToDto(Review review)
        {
            if (review == null)
            {
                return null;
            }

            return new ReviewDTO
            {
                Id = review.Id,
                Description = review.Description,
                Rating = review.Rating,
                SellerId = review.SellerId,
                BuyerId = review.BuyerId,
                Images = review.Images.Select(image => new ImageDTO
                {
                    Url = image.Url
                }).ToList()
            };
        }

        public static Review ToModel(ReviewDTO reviewDTO)
        {
            if (reviewDTO == null)
            {
                return null;
            }

            var review = new Review
            {
                Id = reviewDTO.Id,
                Description = reviewDTO.Description,
                Rating = reviewDTO.Rating,
                SellerId = reviewDTO.SellerId,
                BuyerId = reviewDTO.BuyerId,
                Images = reviewDTO.Images.Select(image => new Image(image.Url)).ToList()
            };

            review.SyncImagesBeforeSave();
            return review;
        }
    }
}