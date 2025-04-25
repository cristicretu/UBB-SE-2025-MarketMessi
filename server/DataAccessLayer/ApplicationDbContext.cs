using Microsoft.EntityFrameworkCore;
using server.Models;
using System;

namespace server.DataAccessLayer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Condition> ProductConditions { get; set; }
        public DbSet<Category> ProductCategories { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<Log> Logs { get; set; }

        // Reviews
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }

        // Auction products
        public DbSet<AuctionProduct> AuctionProducts { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<ProductImage> AuctionProductImages { get; set; }
        public DbSet<AuctionProductProductTag> AuctionProductProductTags { get; set; }

        // Buy products
        public DbSet<BuyProduct> BuyProducts { get; set; }
        public DbSet<BuyProductImage> BuyProductImages { get; set; }
        public DbSet<BuyProductProductTag> BuyProductProductTags { get; set; }

        // Borrow products
        public DbSet<BorrowProduct> BorrowProducts { get; set; }
        public DbSet<BorrowProductImage> BorrowProductImages { get; set; }
        public DbSet<BorrowProductProductTag> BorrowProductProductTags { get; set; }

        // Basket 
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships and constraints
            // Users
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.Email).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Username).IsRequired();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            // Logs
            modelBuilder.Entity<Log>().ToTable("Logs");
            modelBuilder.Entity<Log>().HasKey(l => l.Id);
            modelBuilder.Entity<Log>().Property(l => l.LogLevel).IsRequired();
            modelBuilder.Entity<Log>().Property(l => l.Message).IsRequired();
            modelBuilder.Entity<Log>().Property(l => l.Timestamp).IsRequired();

            // Reviews
            modelBuilder.Entity<Review>().ToTable("Reviews");
            modelBuilder.Entity<Review>().HasKey(r => r.Id);

            modelBuilder.Entity<ReviewImage>().ToTable("ReviewsPictures");
            modelBuilder.Entity<ReviewImage>().HasKey(rp => rp.Id);

            // Product metadata
            modelBuilder.Entity<ProductTag>().ToTable("ProductTags");
            modelBuilder.Entity<ProductTag>().HasKey(pt => pt.Id);
            modelBuilder.Entity<ProductTag>().Property(pt => pt.Title).IsRequired().HasColumnName("title");
            modelBuilder.Entity<ProductTag>().HasIndex(pt => pt.Title).IsUnique();

            // Explicitly ignore any potential relationship properties 
            modelBuilder.Entity<ProductTag>().Ignore("BuyProductId");
            modelBuilder.Entity<ProductTag>().Ignore("BuyProductId1");
            modelBuilder.Entity<ProductTag>().Ignore("BuyProducts");

            modelBuilder.Entity<Condition>().ToTable("ProductConditions");
            modelBuilder.Entity<Condition>().HasKey(pc => pc.Id);
            modelBuilder.Entity<Condition>().HasIndex(pc => pc.Name).IsUnique();

            modelBuilder.Entity<Category>().ToTable("ProductCategories");
            modelBuilder.Entity<Category>().HasKey(pc => pc.Id);
            modelBuilder.Entity<Category>().HasIndex(pc => pc.Name).IsUnique();

            // Auction products
            modelBuilder.Entity<AuctionProduct>().ToTable("AuctionProducts");
            modelBuilder.Entity<AuctionProduct>().HasKey(ap => ap.Id);

            modelBuilder.Entity<ProductImage>().ToTable("AuctionProductsImages");
            modelBuilder.Entity<ProductImage>().HasKey(i => i.Id);

            modelBuilder.Entity<AuctionProductProductTag>().ToTable("AuctionProductProductTags");
            modelBuilder.Entity<AuctionProductProductTag>().HasKey(pt => pt.Id);

            modelBuilder.Entity<Bid>().ToTable("Bids");
            modelBuilder.Entity<Bid>().HasKey(b => b.Id);

            // Buy products
            modelBuilder.Entity<BuyProduct>().ToTable("BuyProducts");
            modelBuilder.Entity<BuyProduct>().HasKey(bp => bp.Id);

            modelBuilder.Entity<BuyProduct>()
                .Ignore(bp => bp.Tags)
                .Ignore(bp => bp.NonMappedImages);

            modelBuilder.Entity<BuyProductImage>().ToTable("BuyProductImages");
            modelBuilder.Entity<BuyProductImage>().HasKey(i => i.Id);
            modelBuilder.Entity<BuyProductImage>().Property(i => i.ProductId).HasColumnName("product_id");
            
            // Configure one-to-many relationship between BuyProduct and BuyProductImage
            modelBuilder.Entity<BuyProductImage>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BuyProductProductTag>().ToTable("BuyProductProductTags");
            modelBuilder.Entity<BuyProductProductTag>().HasKey(pt => pt.Id);
            modelBuilder.Entity<BuyProductProductTag>().Property(pt => pt.ProductId).HasColumnName("product_id");
            modelBuilder.Entity<BuyProductProductTag>().Property(pt => pt.TagId).HasColumnName("tag_id");
            
                
            modelBuilder.Entity<BuyProductProductTag>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BuyProductProductTag>()
                .HasOne(pt => pt.Tag)
                .WithMany()
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Borrow products
            modelBuilder.Entity<BorrowProduct>().ToTable("BorrowProducts");
            modelBuilder.Entity<BorrowProduct>().HasKey(bp => bp.Id);

            modelBuilder.Entity<BorrowProductImage>().ToTable("BorrowProductImages");
            modelBuilder.Entity<BorrowProductImage>().HasKey(i => i.Id);

            modelBuilder.Entity<BorrowProductProductTag>().ToTable("BorrowProductProductTags");
            modelBuilder.Entity<BorrowProductProductTag>().HasKey(pt => pt.Id);

            // Basket
            modelBuilder.Entity<Basket>().ToTable("Baskets");
            modelBuilder.Entity<Basket>().HasKey(b => b.Id);

            // Explicitly ignore the Items collection
            modelBuilder.Entity<Basket>()
                .Ignore(b => b.Items);

            modelBuilder.Entity<BasketItem>().ToTable("BasketItemsBuyProducts");
            modelBuilder.Entity<BasketItem>().HasKey(bi => bi.Id);
            modelBuilder.Entity<BasketItem>()
                .HasOne<BuyProduct>()
                .WithMany()
                .HasForeignKey(bi => bi.ProductId);
        }
    }
}