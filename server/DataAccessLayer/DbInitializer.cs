using MarketMinds.Shared.Models;
using Server.DataAccessLayer;

namespace DataAccessLayer
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Add test user
            if (!context.Users.Any())
            {
                var user = new User
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    PasswordHash = "hashedpassword",
                    Balance = 1000.0,
                    Rating = 4.5,
                    UserType = 0
                };
                context.Users.Add(user);
                context.SaveChanges();
            }

            // Add product conditions
            if (!context.ProductConditions.Any())
            {
                var conditions = new[]
                {
                    new Condition("New", "Brand new, unused item"),
                    new Condition("Like New", "Used once or twice, in excellent condition"),
                    new Condition("Good", "Used but well maintained"),
                    new Condition("Fair", "Shows signs of wear but fully functional")
                };
                context.ProductConditions.AddRange(conditions);
                context.SaveChanges();
            }

            // Add product categories
            if (!context.ProductCategories.Any())
            {
                var categories = new[]
                {
                    new Category { Name = "Electronics", Description = "Electronic devices and gadgets" },
                    new Category { Name = "Books", Description = "Books and publications" },
                    new Category { Name = "Clothing", Description = "Clothes and accessories" },
                    new Category { Name = "Sports", Description = "Sports equipment and gear" }
                };
                context.ProductCategories.AddRange(categories);
                context.SaveChanges();
            }

            // Add auction products
            if (!context.AuctionProducts.Any())
            {
                var seller = context.Users.First();
                var condition = context.ProductConditions.First();
                var category = context.ProductCategories.First();

                var auctionProduct = new AuctionProduct
                {
                    Title = "iPhone 15 Pro",
                    Description = "Brand new iPhone 15 Pro, 256GB",
                    SellerId = seller.Id,
                    ConditionId = condition.Id,
                    CategoryId = category.Id,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddDays(7),
                    StartPrice = 800.0,
                    CurrentPrice = 800.0,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Url = "https://example.com/iphone15.jpg" }
                    }
                };

                context.AuctionProducts.Add(auctionProduct);
                context.SaveChanges();
            }
        }
    }
} 