using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Models;
using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Server.MarketMinds.Repositories.BorrowProductsRepository
{
    public class BorrowProductsRepository : IBorrowProductsRepository
    {
        private readonly ApplicationDbContext context;

        public BorrowProductsRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public List<BorrowProduct> GetProducts()
        {
            try
            {
                // Start with a simpler query if full includes cause issues
                var products = context.BorrowProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .ToList();

                // Optional: Load related entities separately if needed
                foreach (var product in products)
                {
                    try
                    {
                        context.Entry(product)
                            .Collection(p => p.Images)
                            .Load();
                    }
                    catch (Exception imgEx)
                    {
                        Console.WriteLine($"Warning: Could not load images for product {product.Id}: {imgEx.Message}");
                    }

                    try
                    {
                        context.Entry(product)
                            .Collection(p => p.ProductTags)
                            .Load();

                        // Eager load the Tag for each ProductTag
                        foreach (var productTag in product.ProductTags)
                        {
                            try
                            {
                                context.Entry(productTag)
                                    .Reference(pt => pt.Tag)
                                    .Load();
                            }
                            catch (Exception tagEx)
                            {
                                Console.WriteLine($"Warning: Could not load tag for product {product.Id}, tag relation {productTag.Id}: {tagEx.Message}");
                            }
                        }
                    }
                    catch (Exception ptEx)
                    {
                        Console.WriteLine($"Warning: Could not load product tags for product {product.Id}: {ptEx.Message}");
                    }
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProducts: {ex.Message}");
                // Return empty list instead of throwing to prevent cascading failures
                return new List<BorrowProduct>();
            }
        }

        public void DeleteProduct(BorrowProduct product)
        {
            try
            {
                context.BorrowProducts.Remove(product);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteProduct: {ex.Message}");
                throw;
            }
        }

        public void AddProduct(BorrowProduct product)
        {
            try
            {
                context.BorrowProducts.Add(product);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddProduct: {ex.Message}");
                throw;
            }
        }

        public void UpdateProduct(BorrowProduct product)
        {
            try
            {
                var existingProduct = context.BorrowProducts.Find(product.Id);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"BorrowProduct with ID {product.Id} not found for update.");
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
                            context.Set<BorrowProductImage>().Add(image);
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

        public BorrowProduct GetProductByID(int id)
        {
            try
            {
                // Basic product query first
                var product = context.BorrowProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    throw new KeyNotFoundException($"BorrowProduct with ID {id} not found.");
                }

                // Load related entities individually
                try
                {
                    context.Entry(product)
                        .Collection(p => p.Images)
                        .Load();
                }
                catch (Exception imgEx)
                {
                    Console.WriteLine($"Warning: Could not load images for product {id}: {imgEx.Message}");
                    // Initialize empty collection if loading fails
                    product.Images = new List<BorrowProductImage>();
                }

                try
                {
                    context.Entry(product)
                        .Collection(p => p.ProductTags)
                        .Load();

                    // Load related tags
                    foreach (var productTag in product.ProductTags)
                    {
                        try
                        {
                            context.Entry(productTag)
                                .Reference(pt => pt.Tag)
                                .Load();
                        }
                        catch (Exception tagEx)
                        {
                            Console.WriteLine($"Warning: Could not load tag for product {id}, tag relation {productTag.Id}: {tagEx.Message}");
                        }
                    }
                }
                catch (Exception ptEx)
                {
                    Console.WriteLine($"Warning: Could not load product tags for product {id}: {ptEx.Message}");
                    // Initialize empty collection if loading fails
                    product.ProductTags = new List<BorrowProductProductTag>();
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

        public void AddImageToProduct(int productId, BorrowProductImage image)
        {
            try
            {
                // Make sure the product exists
                var product = context.BorrowProducts.Find(productId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"BorrowProduct with ID {productId} not found.");
                }

                // Set the product ID and add the image
                image.ProductId = productId;

                // Add image directly to the DbSet
                context.BorrowProductImages.Add(image);

                // Save changes to the database
                context.SaveChanges();

                Console.WriteLine($"Successfully added image with URL {image.Url} to product {productId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding image to product ID {productId}: {ex.Message}");
                throw new Exception($"Failed to add image to product ID {productId}", ex);
            }
        }
    }
}