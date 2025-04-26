using server.DataAccessLayer; // Assuming DbContext is here
using server.Models; // Using server models
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System;

namespace server.MarketMinds.Repositories.AccountRepository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DataAccessLayer.ApplicationDbContext _context;

        public AccountRepository(DataAccessLayer.ApplicationDbContext context)
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
                    Price = (float)o.Cost,
                    SellerId = o.SellerId,
                    BuyerId = o.BuyerId
                })
                .ToListAsync();

            return orders;
        }

        public async Task<double> GetBasketTotalAsync(int userId, int basketId)
        {
            // Validate parameters
            if (userId <= 0)
                throw new ArgumentException("User ID must be a positive number", nameof(userId));

            if (basketId <= 0)
                throw new ArgumentException("Basket ID must be a positive number", nameof(basketId));

            // Validate the basket belongs to the user
            var basket = await _context.Baskets
                .FirstOrDefaultAsync(b => b.Id == basketId && b.BuyerId == userId);

            if (basket == null)
                throw new ArgumentException($"Basket with ID {basketId} not found for user {userId}", nameof(basketId));

            // Calculate the total cost of all items in the basket
            var basketItems = await _context.BasketItems
                .Where(i => i.BasketId == basketId)
                .ToListAsync();

            if (basketItems == null || !basketItems.Any())
                return 0; // Empty basket has zero cost

            double totalCost = basketItems.Sum(item => item.Price * item.Quantity);
            return totalCost;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null || user.Id <= 0)
                throw new ArgumentException("Valid user must be provided", nameof(user));

            try
            {
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Order>> CreateOrderFromBasketAsync(int userId, int basketId)
        {
            // Validate parameters
            if (userId <= 0)
                throw new ArgumentException("User ID must be a positive number", nameof(userId));

            if (basketId <= 0)
                throw new ArgumentException("Basket ID must be a positive number", nameof(basketId));

            // Get user to make sure they exist
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found", nameof(userId));

            // Load basket first
            var basket = await _context.Baskets.FindAsync(basketId);
            if (basket == null || basket.BuyerId != userId)
                throw new ArgumentException($"Basket with ID {basketId} not found for user {userId}", nameof(basketId));

            // Load basket items separately without using navigation properties
            var basketItems = await _context.BasketItems
                .Where(i => i.BasketId == basketId)
                .ToListAsync();

            if (basketItems == null || !basketItems.Any())
                throw new InvalidOperationException("Cannot create order from empty basket");

            // Load the corresponding products for each basket item
            var productIds = basketItems.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.BuyProducts
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            // Create a dictionary for quick lookup
            var productDictionary = products.ToDictionary(p => p.Id);

            var createdOrders = new List<Order>();

            // Use a transaction to ensure all orders are created or none
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Group by seller
                    var itemsBySeller = basketItems
                        .GroupBy(item =>
                        {
                            // Get the product for this item
                            if (productDictionary.TryGetValue(item.ProductId, out var product))
                                return product.SellerId;
                            // In case product not found, use a fallback
                            return 0; // Default seller ID if product not found
                        });

                    foreach (var sellerGroup in itemsBySeller)
                    {
                        int sellerId = sellerGroup.Key;
                        // Skip orders for invalid seller IDs
                        if (sellerId <= 0)
                            continue;

                        var sellerItems = sellerGroup.ToList();

                        // Calculate total cost for this seller's items
                        double totalCost = sellerItems.Sum(item => item.Price * item.Quantity);

                        // Create detailed description listing all items
                        var itemDescriptions = new List<string>();
                        foreach (var item in sellerItems)
                        {
                            var product = productDictionary[item.ProductId];
                            itemDescriptions.Add($"{product.Title} (x{item.Quantity}) - ${item.Price * item.Quantity:F2}");
                        }
                        string detailedDescription = string.Join(", ", itemDescriptions);

                        // Create order for this seller
                        var order = new Order
                        {
                            Name = $"Order from {user.Username}",
                            Description = detailedDescription,
                            Cost = totalCost,
                            SellerId = sellerId,
                            BuyerId = userId
                        };

                        // Add order to database
                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();

                        createdOrders.Add(order);
                    }

                    // Clear the basket after creating orders
                    _context.BasketItems.RemoveRange(basketItems);
                    await _context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                    return createdOrders;
                }
                catch (Exception ex)
                {
                    // Rollback transaction on any error
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}