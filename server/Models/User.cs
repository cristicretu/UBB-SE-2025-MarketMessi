using System;

namespace server.Models // Adjusted namespace
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int UserType { get; set; } // Consider if this is needed on server
        public float Balance { get; set; } // Consider if this is needed on server
        public float Rating { get; set; } // Consider if this is needed on server
        public float Password { get; set; } // Password should likely not be a float and not stored directly

        private const float MAX_BALANCE = 999999; // Consider if this constant is needed

        // Constructor used by Repository
        public User(int id, string username, string email)
        {
            this.Id = id;
            this.Username = username;
            this.Email = email;
            // this.Balance = MAX_BALANCE; // Removed default balance setting
        }

        // Default constructor for framework needs
        public User() { }
    }
} 