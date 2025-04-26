using System.Threading.Tasks;
using DomainLayer.Domain;

namespace Marketplace_SE.Services
{
    public interface IUserService
    {
        string RetrieveUserId();
        bool ValidateUserId(string enteredId);
        Task<bool> AuthenticateUserAsync(string username, string password);
        Task<User> GetUserByCredentialsAsync(string username, string password);
        Task<bool> IsUsernameTakenAsync(string username);
        Task<bool> RegisterUserAsync(User user);
    }
}
