using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Repositories.UserRepository
{
    public interface IUserRepository
    {
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<User> GetUserByCredentialsAsync(string username, string password);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> IsUsernameTakenAsync(string username);
        Task<int> CreateUserAsync(User newUser);
    }
} 