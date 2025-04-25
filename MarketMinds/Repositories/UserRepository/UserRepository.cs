using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Repositories.UserRepository
{
    public class UserRepository
    {
        private readonly List<User> users;

        public UserRepository()
        {
            users = new List<User>
        {
            new User(1, "john_doe", "john@example.com", "token123")
            {
                UserType = 1,
                Balance = 1000f,
                Rating = 4.5f,
                Password = "1234"
            },
            new User(2, "jane_doe", "jane@example.com", "token456")
            {
                UserType = 2,
                Balance = 2500f,
                Rating = 4.8f,
                Password = "1234"
            }
        };
        }

        public bool ValidateCredentials(string username, string password)
        {
            // Hardcoded password check — fix your model to use string & secure password logic
            return users.Any(u => u.Username == username && u.Password == password);
        }

        public User GetUserByUsername(string username)
        {
            return users.FirstOrDefault(u => u.Username == username);
        }

        public User GetUserByEmail(string email)
        {
            return users.FirstOrDefault(u => u.Email == email);
        }

        public User GetUserByCredentials(string username, string password)
        {
            return users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public void UpdateUser(User updatedUser)
        {
            var index = users.FindIndex(u => u.Id == updatedUser.Id);
            if (index != -1)
            {
                users[index] = updatedUser;
            }
        }

        public int CreateUser(User newUser)
        {
            newUser.Id = users.Max(u => u.Id) + 1;
            users.Add(newUser);
            return newUser.Id;
        }

        public bool ResetUserPassword(string email, string newPassword)
        {
            var user = GetUserByEmail(email);
            if (user == null)
            {
                return false;
            }

            user.Password = newPassword;
            UpdateUser(user);
            return true;
        }
    }
}
