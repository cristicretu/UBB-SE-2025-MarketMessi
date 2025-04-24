using System;
using System.Collections.Generic;

namespace server.Models.DTOs
{
    public class BuyProductDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public ConditionDTO Condition { get; set; }
        public CategoryDTO Category { get; set; }
        public List<ProductTagDTO> Tags { get; set; } = new List<ProductTagDTO>();
        public List<ImageDTO> Images { get; set; } = new List<ImageDTO>();
        public UserDTO Seller { get; set; }
    }
}

public class ConditionDTO
{
    public int Id { get; set; }
    public string DisplayTitle { get; set; }
    public string Description { get; set; }
}

public class CategoryDTO
{
    public int Id { get; set; }
    public string DisplayTitle { get; set; }
    public string Description { get; set; }
}

public class ProductTagDTO
{
    public int Id { get; set; }
    public string DisplayTitle { get; set; }
}

public class ImageDTO
{
    public string Url { get; set; }
}

public class UserDTO
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public int UserType { get; set; }
    public double Balance { get; set; }
    public double Rating { get; set; }
    public int Password { get; set; } 
}
