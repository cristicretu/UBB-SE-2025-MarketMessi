using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using MarketMinds.Shared.Models;

namespace MarketMinds.Repositories.AuctionProductsRepository
{
    public class AuctionProductsRepository : IAuctionProductsRepository
    {
        private readonly ApplicationDbContext context;

        public AuctionProductsRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public List<AuctionProduct> GetProducts()
        {
            try
            {
                var products = context.AuctionProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Bids)
                        .ThenInclude(b => b.Bidder)
                    .Include(p => p.Images)
                    .ToList();

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProducts: {ex.Message}");
                throw;
            }
        }

        public void DeleteProduct(AuctionProduct product)
        {
            try
            {
                context.AuctionProducts.Remove(product);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteProduct: {ex.Message}");
                throw;
            }
        }

        public void AddProduct(AuctionProduct product)
        {
            try
            {
                context.AuctionProducts.Add(product);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddProduct: {ex.Message}");
                throw;
            }
        }

        public void UpdateProduct(AuctionProduct product)
        {
            try
            {
                var existingProduct = context.AuctionProducts.Find(product.Id);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"AuctionProduct with ID {product.Id} not found for update.");
                }

                context.Entry(existingProduct).CurrentValues.SetValues(product);

                if (product.Images != null && product.Images.Any())
                {
                    Console.WriteLine($"Updating product {product.Id} with {product.Images.Count} images");

                    foreach (var image in product.Images)
                    {
                        if (image.Id == 0)
                        {
                            Console.WriteLine($"Adding new image with URL: {image.Url} to product ID: {product.Id}");
                            image.ProductId = product.Id;
                            context.Set<ProductImage>().Add(image);
                        }
                    }
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateProduct: {ex.Message}");
                throw;
            }
        }

        public AuctionProduct GetProductByID(int id)
        {
            try
            {
                var product = context.AuctionProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Bids)
                        .ThenInclude(b => b.Bidder)
                    .Include(p => p.Images)
                    .FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    throw new KeyNotFoundException($"AuctionProduct with ID {id} not found.");
                }

                return product;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProductByID: {ex.Message}");
                throw;
            }
        }
    }
}