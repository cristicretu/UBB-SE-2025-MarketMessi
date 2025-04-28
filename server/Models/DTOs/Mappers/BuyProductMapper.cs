using System.Linq;
using System.Collections.Generic;
using Server.Models;
using Server.Models.DTOs;

namespace Server.Models.DTOs.Mappers
{
    public static class BuyProductMapper
    {
        private static int UNDEFINED_USER_TYPE = 0;
        private static int BASE_BALANCE = 0;
        private static int BASE_RATING = 0;
        private static int UNDEFINED_PASSWORD = 0;
        public static BuyProductDTO ToDTO(BuyProduct buyProduct)
        {
            if (buyProduct == null)
            {
                return null;
            }

            return new BuyProductDTO
            {
                Id = buyProduct.Id,
                Title = buyProduct.Title,
                Description = buyProduct.Description,
                Price = buyProduct.Price,
                Condition = buyProduct.Condition != null ? new ConditionDTO
                {
                    Id = buyProduct.Condition.Id,
                    DisplayTitle = buyProduct.Condition.Name,
                    Description = null
                }
                : null,
                Category = buyProduct.Category != null ? new CategoryDTO
                {
                    Id = buyProduct.Category.Id,
                    DisplayTitle = buyProduct.Category.Name,
                    Description = buyProduct.Category.Description
                }
                : null,
                Tags = buyProduct.ProductTags?.Select(productTag => new ProductTagDTO
                {
                    Id = productTag.Tag.Id,
                    DisplayTitle = productTag.Tag.Title
                }).ToList() ?? new List<ProductTagDTO>(),
                Images = buyProduct.Images?.Select(image => new ImageDTO
                {
                    Url = image.Url
                }).ToList() ??
                buyProduct.NonMappedImages?.Select(image => new ImageDTO
                {
                    Url = image.Url
                }).ToList() ??
                new List<ImageDTO>(),
                Seller = buyProduct.Seller != null ? new UserDTO
                {
                    Id = buyProduct.Seller.Id,
                    Username = buyProduct.Seller.Username,
                    Email = buyProduct.Seller.Email,
                    UserType = UNDEFINED_USER_TYPE,
                    Balance = BASE_BALANCE,
                    Rating = BASE_RATING,
                    Password = UNDEFINED_PASSWORD
                }
                : null
            };
        }

        public static List<BuyProductDTO> ToDTOList(IEnumerable<BuyProduct> buyProducts)
        {
            return buyProducts?.Select(buyProduct => ToDTO(buyProduct)).ToList() ?? new List<BuyProductDTO>();
        }
    }
}