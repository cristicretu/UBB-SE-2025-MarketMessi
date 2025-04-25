using System.Collections.Generic;
using System;
using System.Linq;
using server.Models;
using server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace MarketMinds.Repositories.BuyProductsRepository
{
    public class BuyProductsRepository: IBuyProductsRepository
    {
        private readonly ApplicationDbContext _context;

        public BuyProductsRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void AddProduct(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                _context.BuyProducts.Add(product);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core AddProduct Error: {ex.InnerException?.Message ?? ex.Message}");
                throw new Exception("Failed to add product to the database", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General AddProduct Error: {ex.Message}");
                throw;
            }
        }

        public void DeleteProduct(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                var productToDelete = _context.BuyProducts.Find(product.Id);
                if (productToDelete == null)
                {
                    throw new KeyNotFoundException($"Product with ID {product.Id} not found for deletion.");
                }
                _context.BuyProducts.Remove(productToDelete);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core DeleteProduct Error: {ex.InnerException?.Message ?? ex.Message}");
                throw new Exception("Failed to delete product from the database", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General DeleteProduct Error: {ex.Message}");
                throw;
            }
        }

        public List<BuyProduct> GetProducts()
        {
            try
            {
                return _context.BuyProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    // Add these back one by one after confirming basic query works
                    .Include(p => p.Images)
                    // .Include(p => p.ProductTags)
                    //    .ThenInclude(pt => pt.Tag)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting BuyProducts: {ex.Message}");
                throw new Exception("Failed to retrieve products from the database", ex);
            }
        }

        public BuyProduct GetProductByID(int productId)
        {
            try
            {
                var product = _context.BuyProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefault(p => p.Id == productId);

                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {productId} not found.");
                }
                return product;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting BuyProduct by ID {productId}: {ex.Message}");
                throw new Exception($"Failed to retrieve product with ID {productId}", ex);
            }
        }

        public void UpdateProduct(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                var existingProduct = _context.BuyProducts
                    .Include(p => p.Images)
                    .Include(p => p.ProductTags)
                    .FirstOrDefault(p => p.Id == product.Id);

                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"Product with ID {product.Id} not found for update.");
                }

                _context.Entry(existingProduct).CurrentValues.SetValues(product);

                var imagesToRemove = existingProduct.Images
                    .Where(dbImg => !product.Images.Any(pImg => pImg.Id == dbImg.Id && pImg.Id != 0))
                    .ToList();
                _context.Set<BuyProductImage>().RemoveRange(imagesToRemove);

                foreach (var image in product.Images)
                {
                    var existingImage = existingProduct.Images.FirstOrDefault(i => i.Id == image.Id && i.Id != 0);
                    if (existingImage == null)
                    {
                        image.ProductId = existingProduct.Id;
                        image.Id = 0;
                        _context.Set<BuyProductImage>().Add(image);
                    }
                }

                var tagsToRemove = existingProduct.ProductTags
                    .Where(dbPt => !product.ProductTags.Any(pPt => pPt.TagId == dbPt.TagId))
                    .ToList();
                _context.Set<BuyProductProductTag>().RemoveRange(tagsToRemove);

                foreach (var productTag in product.ProductTags)
                {
                    var existingLink = existingProduct.ProductTags.FirstOrDefault(pt => pt.TagId == productTag.TagId);
                    if (existingLink == null)
                    {
                        _context.Set<BuyProductProductTag>().Add(new BuyProductProductTag
                        {
                            ProductId = existingProduct.Id,
                            TagId = productTag.TagId
                        });
                    }
                }

                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Concurrency error updating product {product.Id}: {ex.Message}");
                throw new Exception($"Failed to update product with ID {product.Id} due to concurrency conflict", ex);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core UpdateProduct Error: {ex.InnerException?.Message ?? ex.Message}");
                throw new Exception($"Failed to update product with ID {product.Id}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General UpdateProduct Error for ID {product.Id}: {ex.Message}");
                throw;
            }
        }
    }
}
