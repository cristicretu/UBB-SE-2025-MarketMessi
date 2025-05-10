using System.Collections.Generic;

namespace MarketMinds.Shared.Models.DTOs
{
    public class BuyProductBasketDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public UserDTO Seller { get; set; }
        public ConditionDTO Condition { get; set; }
        public CategoryDTO Category { get; set; }
        public List<ImageDTO> Images { get; set; } = new List<ImageDTO>();
    }
} 