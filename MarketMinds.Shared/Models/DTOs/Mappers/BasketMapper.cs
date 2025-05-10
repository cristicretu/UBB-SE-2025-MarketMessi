using System.Collections.Generic;
using System.Linq;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;

namespace MarketMinds.Shared.Models.DTOs.Mappers
{
    public static class BasketMapper
    {
        private static int undefined_user_type = 0;
        private static int undefined_balance = 0;
        private static int undefined_rating = 0;
        private static int undefined_password = 0;
        public static BasketDTO ToDTO(Basket entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new BasketDTO
            {
                Id = entity.Id,
                BuyerId = entity.BuyerId,
                Items = entity.Items?.Select(i => new BasketItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    BasketId = i.BasketId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Product = i.Product != null ? new ProductDTO
                    {
                        Id = i.Product.Id,
                        Title = i.Product.Title,
                        Description = i.Product.Description,
                        Price = i.Product.Price,
                        Seller = i.Product.Seller != null ? new UserDTO
                        {
                            Id = i.Product.Seller.LegacyId,
                            Username = i.Product.Seller.Username,
                            Email = i.Product.Seller.Email,
                            UserType = 0,
                            Balance = 0,
                            Rating = 0,
                            Password = 0
                        } : null,
                        Condition = i.Product.Condition != null ? new ConditionDTO
                        {
                            Id = i.Product.Condition.Id,
                            DisplayTitle = i.Product.Condition.Name,
                            Description = i.Product.Condition.Description
                        } : null,
                        Category = i.Product.Category != null ? new CategoryDTO
                        {
                            Id = i.Product.Category.Id,
                            DisplayTitle = i.Product.Category.Name,
                            Description = i.Product.Category.Description
                        } : null,
                        Images = i.Product.Images?.Select(img => new ImageDTO
                        {
                            Url = img.Url
                        }).ToList() ?? new List<ImageDTO>(),
                        Tags = new List<ProductTagDTO>()
                    } : null
                }).ToList() ?? new List<BasketItemDTO>()
            };
        }

        public static List<BasketDTO> ToDTOList(IEnumerable<Basket> entities)
        {
            return entities?.Select(e => ToDTO(e)).ToList() ?? new List<BasketDTO>();
        }

        public static BasketItemDTO ToDTO(BasketItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new BasketItemDTO
            {
                Id = item.Id,
                BasketId = item.BasketId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                Product = item.Product != null ? ToProductDTO(item.Product) : null
            };
        }

        public static ProductDTO ToProductDTO(BuyProduct product)
        {
            if (product == null)
            {
                return null;
            }

            return new ProductDTO
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                Seller = product.Seller != null ? new UserDTO
                {
                    Id = product.Seller.LegacyId,
                    Username = product.Seller.Username,
                    Email = product.Seller.Email,
                    // Set default values for other UserDTO properties
                    UserType = undefined_user_type,
                    Balance = undefined_balance,
                    Rating = undefined_rating,
                    Password = undefined_password
                }
                : null,
                Condition = product.Condition != null ? new ConditionDTO
                {
                    Id = product.Condition.Id,
                    DisplayTitle = product.Condition.Name,
                    Description = string.Empty
                }
                : null,
                Category = product.Category != null ? new CategoryDTO
                {
                    Id = product.Category.Id,
                    DisplayTitle = product.Category.Name,
                    Description = product.Category.Description
                }
                : null,
                Tags = product.Tags?.Select(tag => new ProductTagDTO
                {
                    Id = tag.Id,
                    DisplayTitle = tag.Title
                }).ToList() ?? new List<ProductTagDTO>(),
                Images = product.Images?.Select(image => new ImageDTO
                {
                    Url = image.Url
                }).ToList() ?? new List<ImageDTO>()
            };
        }
    }
}