using server.Models;

namespace server.MarketMinds.Repositories.AccountRepository
{
    public interface IAccountRepository
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<List<UserOrder>> GetUserOrdersAsync(int userId);
        Task<List<Order>> CreateOrderFromBasketAsync(int userId, int basketId);
        Task<double> GetBasketTotalAsync(int userId, int basketId);
        Task<bool> UpdateUserAsync(User user);
    }
}