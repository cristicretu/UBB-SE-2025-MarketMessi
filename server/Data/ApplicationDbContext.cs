using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AuctionProduct> AuctionProducts { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductTag> Tags { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.SellingItems)
                .WithOne(p => p.Seller)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Bids)
                .WithOne(b => b.Bidder)
                .HasForeignKey(b => b.BidderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuctionProduct>()
                .HasMany(p => p.Bids)
                .WithOne(b => b.Product)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuctionProduct>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Subcategories)
                .WithOne(c => c.Parent)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Condition>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Condition)
                .HasForeignKey(p => p.ConditionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 