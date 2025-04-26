using server.DataAccessLayer; // Assuming DbContext is here
using server.Models; // Changed to use server models
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Repositories
{
    public interface IAccountRepository
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<List<UserOrder>> GetUserOrdersAsync(int userId);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;

        public AccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user; 
        }

        public async Task<List<UserOrder>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.SellerId == userId || o.BuyerId == userId)
                .OrderByDescending(o => o.Id) 
                .Select(o => new UserOrder 
                {
                    Id = o.Id,
                    ItemName = o.Name, 
                    Description = o.Description,
                    Price = o.Cost, 
                    SellerId = o.SellerId,
                    BuyerId = o.BuyerId,
                    Created = o.Id 
                })
                .ToListAsync();

            return orders;
        }
    }
} 