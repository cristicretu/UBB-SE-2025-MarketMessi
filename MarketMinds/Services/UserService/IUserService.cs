using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.UserService
{
    public interface IUserService
    {
        Task<bool> AuthenticateUserAsync(string username, string password);
        Task<User> GetUserByCredentialsAsync(string username, string password);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> IsUsernameTakenAsync(string username);
        Task<bool> RegisterUserAsync(User user);
        Task<User> GetUserByIdAsync(int userId);
    }
}