using System.Linq;
using System.Collections.Generic;
using server.Models;
using server.Models.DTOs;

namespace server.Models.DTOs.Mappers
{
    public static class AuctionProductMapper
    {
        public static AuctionProductDTO ToDTO(AuctionProduct entity)
        {
            if (entity == null) return null;

            return new AuctionProductDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                StartAuctionDate = entity.StartTime,
                EndAuctionDate = entity.EndTime,
                StartingPrice = entity.StartPrice,
                CurrentPrice = entity.CurrentPrice,
                BidHistory = entity.Bids?.Select(b => new BidDTO
                {
                    Id = b.Id,
                    Price = b.Price,
                    Timestamp = b.Timestamp,
                    Bidder = b.Bidder != null ? new UserDTO
                    {
                        Id = b.Bidder.Id,
                        Username = b.Bidder.Username,
                        Email = b.Bidder.Email,
                        UserType = 0,
                        Balance = 0,
                        Rating = 0,
                        Password = 0
                    } : null
                }).ToList() ?? new List<BidDTO>(),
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
                Tags = new List<ProductTagDTO>(), 
                Images = entity.Images?.Select(i => new ImageDTO
                {
                    Url = i.Url
                }).ToList() ?? new List<ImageDTO>(),
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

        public static List<AuctionProductDTO> ToDTOList(IEnumerable<AuctionProduct> entities)
        {
            return entities?.Select(e => ToDTO(e)).ToList() ?? new List<AuctionProductDTO>();
        }
    }
} 