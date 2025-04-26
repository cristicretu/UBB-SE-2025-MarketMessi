using System;
using System.Text.Json.Serialization;

namespace MarketMinds.Models
{
    public class UserDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
        
        [JsonPropertyName("userType")]
        public int UserType { get; set; }
        
        [JsonPropertyName("balance")]
        public float Balance { get; set; }
        
        [JsonPropertyName("rating")]
        public float Rating { get; set; }
        
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        public UserDto()
        {
        }

        public UserDto(int id, string username, string email, string token = "")
        {
            Id = id;
            Username = username;
            Email = email;
            Token = token;
            UserType = 0;
            Balance = 0;
            Rating = 0;
        }

        public UserDto(DomainLayer.Domain.User user)
        {
            Id = user.Id;
            Username = user.Username;
            Email = user.Email;
            Password = user.Password;
            UserType = user.UserType;
            Balance = user.Balance;
            Rating = user.Rating;
            Token = user.Token;
        }

        public DomainLayer.Domain.User ToDomainUser()
        {
            var user = new DomainLayer.Domain.User(Id, Username, Email, Token)
            {
                UserType = UserType,
                Balance = Balance,
                Rating = Rating,
                Password = Password
            };
            
            return user;
        }
    }
} 