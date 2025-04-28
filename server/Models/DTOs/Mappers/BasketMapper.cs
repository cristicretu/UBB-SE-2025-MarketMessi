using System.Collections.Generic;
using System.Linq;
using Server.Models;
using Server.Models.DTOs;
using MarketMinds.Controllers;

namespace Server.Models.DTOs.Mappers
{
    public static class BasketMapper
    {
        private static int BASE_BALANCE = 0;
        private static int BASE_RATING = 0;
        private static int UNDEFINED_USER_TYPE = 0;
        private static int UNDEFINED_PASSWORD = 0;
        public static BasketDTO ToDTO(Basket basket)
        {
            if (basket == null)
            {
                return null;
            }

            return new BasketDTO
            {
                Id = basket.Id,
                BuyerId = basket.BuyerId,
                Items = basket.Items?.Select(item => ToDTO(item)).ToList() ?? new List<BasketItemDTO>()
            };
        }

        public static BasketItemDTO ToDTO(BasketItem basketItem)
        {
            if (basketItem == null)
            {
                return null;
            }

            return new BasketItemDTO
            {
                Id = basketItem.Id,
                BasketId = basketItem.BasketId,
                ProductId = basketItem.ProductId,
                Quantity = basketItem.Quantity,
                Price = basketItem.Price,
                Product = basketItem.Product != null ? ToProductDTO(basketItem.Product) : null
            };
        }

        public static ProductDTO ToProductDTO(BuyProduct buyProduct)
        {
            if (buyProduct == null)
            {
                return null;
            }

            return new ProductDTO
            {
                Id = buyProduct.Id,
                Title = buyProduct.Title,
                Description = buyProduct.Description,
                Price = buyProduct.Price,
                Seller = buyProduct.Seller != null ? new UserDTO
                {
                    Id = buyProduct.Seller.Id,
                    Username = buyProduct.Seller.Username,
                    Email = buyProduct.Seller.Email,
                    // Set default values for other UserDTO properties
                    UserType = UNDEFINED_USER_TYPE,
                    Balance = BASE_BALANCE,
                    Rating = BASE_RATING,
                    Password = UNDEFINED_PASSWORD
                }
                : null,
                Condition = buyProduct.Condition != null ? new ConditionDTO
                {
                    Id = buyProduct.Condition.Id,
                    DisplayTitle = buyProduct.Condition.Name,
                    Description = string.Empty
                }
                : null,
                Category = buyProduct.Category != null ? new CategoryDTO
                {
                    Id = buyProduct.Category.Id,
                    DisplayTitle = buyProduct.Category.Name,
                    Description = buyProduct.Category.Description
                }
                : null,
                Tags = buyProduct.Tags?.Select(productTag => new ProductTagDTO
                {
                    Id = productTag.Id,
                    DisplayTitle = productTag.Title
                }).ToList() ?? new List<ProductTagDTO>(),
                Images = buyProduct.Images?.Select(image => new ImageDTO
                {
                    Url = image.Url
                }).ToList() ?? new List<ImageDTO>()
            };
        }

        public static BasketTotalsDTO ToDTO(BasketController.BasketTotals totals)
        {
            if (totals == null)
            {
                return null;
            }

            return new BasketTotalsDTO
            {
                Subtotal = totals.Subtotal,
                Discount = totals.Discount,
                TotalAmount = totals.TotalAmount
            };
        }

        public static List<BasketDTO> ToDTOList(IEnumerable<Basket> baskets)
        {
            return baskets?.Select(basket => ToDTO(basket)).ToList() ?? new List<BasketDTO>();
        }
    }
}