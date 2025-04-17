using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Repositories
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AuctionProduct> AuctionProducts { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<ProductCondition> ProductConditions { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<AuctionProduct>()
                .HasOne(p => p.Seller)
                .WithMany()
                .HasForeignKey(p => p.SellerId);

            modelBuilder.Entity<AuctionProduct>()
                .HasOne(p => p.Condition)
                .WithMany()
                .HasForeignKey(p => p.ConditionId);

            modelBuilder.Entity<AuctionProduct>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Bidder)
                .WithMany()
                .HasForeignKey(b => b.BidderId);

            modelBuilder.Entity<Bid>()
                .HasOne<AuctionProduct>()
                .WithMany()
                .HasForeignKey(b => b.ProductId);
        }
    }
} 