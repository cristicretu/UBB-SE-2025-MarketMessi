using System.Linq;
using System.Collections.Generic;
using Server.Models;
using Server.Models.DTOs;

namespace Server.Models.DTOs.Mappers
{
    public static class AuctionProductMapper
    {
        private static int UNDEFINED_USER_TYPE = 0;
        private static int BASE_BALANCE = 0;
        private static int BASE_RATING = 0;
        private static int UNDEFINED_PASSWORD = 0;
        public static AuctionProductDTO ToDTO(AuctionProduct auctionProduct)
        {
            if (auctionProduct == null)
            {
                return null;
            }

            return new AuctionProductDTO
            {
                Id = auctionProduct.Id,
                Title = auctionProduct.Title,
                Description = auctionProduct.Description,
                StartAuctionDate = auctionProduct.StartTime,
                EndAuctionDate = auctionProduct.EndTime,
                StartingPrice = auctionProduct.StartPrice,
                CurrentPrice = auctionProduct.CurrentPrice,
                BidHistory = auctionProduct.Bids?.Select(bid => new BidDTO
                {
                    Id = bid.Id,
                    Price = bid.Price,
                    Timestamp = bid.Timestamp,
                    Bidder = bid.Bidder != null ? new UserDTO
                    {
                        Id = bid.Bidder.Id,
                        Username = bid.Bidder.Username,
                        Email = bid.Bidder.Email,
                        UserType = UNDEFINED_USER_TYPE,
                        Balance = BASE_BALANCE,
                        Rating = BASE_RATING,
                        Password = UNDEFINED_PASSWORD
                    }
                    : null
                }).ToList() ?? new List<BidDTO>(),
                Condition = auctionProduct.Condition != null ? new ConditionDTO
                {
                    Id = auctionProduct.Condition.Id,
                    DisplayTitle = auctionProduct.Condition.Name,
                    Description = null
                }
                : null,
                Category = auctionProduct.Category != null ? new CategoryDTO
                {
                    Id = auctionProduct.Category.Id,
                    DisplayTitle = auctionProduct.Category.Name,
                    Description = auctionProduct.Category.Description
                }
                : null,
                Tags = new List<ProductTagDTO>(),
                Images = auctionProduct.Images?.Select(i => new ImageDTO
                {
                    Url = i.Url
                }).ToList() ?? new List<ImageDTO>(),
                Seller = auctionProduct.Seller != null ? new UserDTO
                {
                    Id = auctionProduct.Seller.Id,
                    Username = auctionProduct.Seller.Username,
                    Email = auctionProduct.Seller.Email,
                    UserType = UNDEFINED_USER_TYPE,
                    Balance = BASE_BALANCE,
                    Rating = BASE_RATING,
                    Password = UNDEFINED_PASSWORD
                }
                : null
            };
        }

        public static List<AuctionProductDTO> ToDTOList(IEnumerable<AuctionProduct> auctionProducts)
        {
            return auctionProducts?.Select(auctionProduct => ToDTO(auctionProduct)).ToList() ?? new List<AuctionProductDTO>();
        }
    }
}