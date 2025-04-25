using System;
using System.Collections.Generic;
using System.Linq;
using server.Models;
using System.Threading.Tasks;
using server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace server.MarketMinds.Repositories.BorrowProductsRepository
{
    public class BorrowProductsRepository : IBorrowProductsRepository
    {
        private readonly ApplicationDbContext _context;

        public BorrowProductsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<BorrowProduct> GetProducts()
        {
            try
            {
                // Start with a simpler query if full includes cause issues
                var products = _context.BorrowProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .ToList();

                // Optional: Load related entities separately if needed
                foreach (var product in products)
                {
                    try
                    {
                        _context.Entry(product)
                            .Collection(p => p.Images)
                            .Load();
                    }
                    catch (Exception imgEx)
                    {
                        Console.WriteLine($"Warning: Could not load images for product {product.Id}: {imgEx.Message}");
                    }

                    try
                    {
                        _context.Entry(product)
                            .Collection(p => p.ProductTags)
                            .Load();

                        // Eager load the Tag for each ProductTag
                        foreach (var productTag in product.ProductTags)
                        {
                            try
                            {
                                _context.Entry(productTag)
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
                _context.BorrowProducts.Remove(product);
                _context.SaveChanges();
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
                _context.BorrowProducts.Add(product);
                _context.SaveChanges();
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
                var existingProduct = _context.BorrowProducts.Find(product.Id);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"BorrowProduct with ID {product.Id} not found for update.");
                }

                _context.Entry(existingProduct).CurrentValues.SetValues(product);

                if (product.Images != null && product.Images.Any())
                {
                    Console.WriteLine($"Updating product {product.Id} with {product.Images.Count} images");
                    
                    foreach (var image in product.Images)
                    {
                        if (image.Id == 0)
                        {
                            Console.WriteLine($"Adding new image with URL: {image.Url} to product ID: {product.Id}");
                            image.ProductId = product.Id;
                            _context.Set<BorrowProductImage>().Add(image);
                        }
                    }
                }

                _context.SaveChanges();
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
                var product = _context.BorrowProducts
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
                    _context.Entry(product)
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
                    _context.Entry(product)
                        .Collection(p => p.ProductTags)
                        .Load();

                    // Load related tags
                    foreach (var productTag in product.ProductTags)
                    {
                        try
                        {
                            _context.Entry(productTag)
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
                var product = _context.BorrowProducts.Find(productId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"BorrowProduct with ID {productId} not found.");
                }

                // Set the product ID and add the image
                image.ProductId = productId;
                
                // Add image directly to the DbSet
                _context.BorrowProductImages.Add(image);
                
                // Save changes to the database
                _context.SaveChanges();
                
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