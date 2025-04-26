using System.Threading.Tasks;
using DomainLayer.Domain;

namespace MarketMinds.Services.AuthService
{
    public interface IAuthService
    {
        Task<bool> AuthenticateUserAsync(string username, string password);
        Task<User> GetUserByCredentialsAsync(string username, string password);
        Task<bool> IsUsernameTakenAsync(string username);
        Task<bool> RegisterUserAsync(User user);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
    }
} 