using System;

namespace DomainLayer.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int UserType { get; set; }
        public float Balance { get; set; }
        public float Rating { get; set; }
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        private const float MAX_BALANCE = 999999;

        public User()
        {
            Id = 0;
            Username = string.Empty;
            Email = string.Empty;
            UserType = 0;
            Balance = 0;
            Rating = 0;
            Password = string.Empty;
            Token = string.Empty;
        }

        // Constructor with parameters
        public User(int id, string username, string email, string token = "")
        {
            Id = id;
            Username = username;
            Email = email;
            UserType = 0;
            Balance = 0;
            Rating = 0;
            Password = string.Empty;
            Token = token;
        }

        public int GetId()
        {
            return Id;
        }
    }
}
