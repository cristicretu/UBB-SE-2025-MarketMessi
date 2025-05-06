using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models;
namespace MarketMinds.Shared.Services
{
    public interface IAccountPageService
    {
        Task<User> GetUserAsync(int userId);
        Task<User> GetCurrentLoggedInUserAsync();
        Task<List<UserOrder>> GetUserOrdersAsync(int userId);
    }
}