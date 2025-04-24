using System;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;

namespace MarketMinds.Services.LoginService
{
    public class LoginService
    {
        private readonly UserRepository userRepository;

        public LoginService()
        {
            userRepository = new UserRepository();
        }

        public bool AuthenticateUser(string username, string password)
        {
            return userRepository.ValidateCredentials(username, password);
        }

        public User GetUserByCredentials(string username, string password)
        {
            if (AuthenticateUser(username, password))
            {
                var user = userRepository.GetUserByUsername(username);
                userRepository.UpdateUser(user);
            }
            return userRepository.GetUserByCredentials(username, password);
        }
    }
}