using System.Linq;
using System.Collections.Generic;
using server.Models;
using server.Models.DTOs;

namespace server.Models.DTOs.Mappers
{
    public static class BuyProductMapper
    {
        public static BuyProductDTO ToDTO(BuyProduct entity)
        {
            if (entity == null) return null;

            return new BuyProductDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Price = entity.Price,
                Condition = entity.Condition != null ? new ConditionDTO
                {
                    Id = entity.Condition.Id,
                    DisplayTitle = entity.Condition.Name,
                    Description = null
                } : null,
                Category = entity.Category != null ? new CategoryDTO
                {
                    Id = entity.Category.Id,
                    DisplayTitle = entity.Category.Name,
                    Description = entity.Category.Description
                } : null,
                Tags = entity.ProductTags?.Select(pt => new ProductTagDTO
                {
                    Id = pt.Tag.Id,
                    DisplayTitle = pt.Tag.Title
                }).ToList() ?? new List<ProductTagDTO>(),
                Images = entity.Images?.Select(i => new ImageDTO
                {
                    Url = i.Url
                }).ToList() ?? 
                entity.NonMappedImages?.Select(i => new ImageDTO
                {
                    Url = i.Url
                }).ToList() ?? 
                new List<ImageDTO>(),
                Seller = entity.Seller != null ? new UserDTO
                {
                    Id = entity.Seller.Id,
                    Username = entity.Seller.Username,
                    Email = entity.Seller.Email,
                    UserType = 0,
                    Balance = 0,
                    Rating = 0,
                    Password = 0
                } : null
            };
        }

        public static List<BuyProductDTO> ToDTOList(IEnumerable<BuyProduct> entities)
        {
            return entities?.Select(e => ToDTO(e)).ToList() ?? new List<BuyProductDTO>();
        }
    }
}