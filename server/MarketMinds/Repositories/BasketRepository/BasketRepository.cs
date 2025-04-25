using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using server.DataAccessLayer;
using server.Models;
using System.Diagnostics;

namespace MarketMinds.Repositories.BasketRepository
{
    public class BasketRepository : IBasketRepository
    {
        private const int NOBASKET = -1;
        private const int NOITEM = -1;
        private const int DEFAULTPRICE = 0;
        private const int MINIMUMID = 0;
        private const int NOQUANTITY = 0;
        private readonly ApplicationDbContext _context;

        public BasketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Basket GetBasketByUserId(int userId)
        {
            // Try to find existing basket for the user
            var basketEntity = _context.Baskets
                .FirstOrDefault(b => b.BuyerId == userId);

            // If no basket exists, create one
            if (basketEntity == null)
            {
                basketEntity = new Basket
                {
                    BuyerId = userId
                };
                _context.Baskets.Add(basketEntity);
                _context.SaveChanges();
            }

            // Load basket items
            basketEntity.Items = GetBasketItems(basketEntity.Id);

            return basketEntity;
        }

        public void RemoveItemByProductId(int basketId, int productId)
        {
            var basketItem = _context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (basketItem != null)
            {
                _context.BasketItems.Remove(basketItem);
                _context.SaveChanges();
            }
        }

        public List<BasketItem> GetBasketItems(int basketId)
        {
            Debug.WriteLine($"[Repository] GetBasketItems called with basketId: {basketId}");

            // Get basket items - don't include Product since it's NotMapped
            var basketItems = _context.BasketItems
                .Where(bi => bi.BasketId == basketId)
                .ToList();

            Debug.WriteLine($"[Repository] Found {basketItems.Count} basket items in database");

            // For each item, load its product details separately
            foreach (var item in basketItems)
            {
                Debug.WriteLine($"[Repository] Loading product details for item {item.Id}, productId: {item.ProductId}");

                // Load the product details
                var product = _context.BuyProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Id == item.ProductId);

                if (product != null)
                {
                    Debug.WriteLine($"[Repository] Found product: {product.Id} - {product.Title}");
                    item.Product = product;

                    // Ensure ProductId matches the product's ID
                    if (item.ProductId != product.Id)
                    {
                        Debug.WriteLine($"[Repository] WARNING: ProductId mismatch! Setting ProductId={product.Id} for item {item.Id}");
                        item.ProductId = product.Id;
                    }

                    // Load tags
                    var productTagIds = _context.Set<BuyProductProductTag>()
                        .Where(pt => pt.ProductId == product.Id)
                        .Select(pt => pt.TagId)
                        .ToList();

                    var tags = _context.Set<ProductTag>()
                        .Where(t => productTagIds.Contains(t.Id))
                        .ToList();

                    product.Tags = tags;
                    Debug.WriteLine($"[Repository] Loaded {tags.Count} tags for product {product.Id}");

                    // Load images
                    var productImages = _context.Set<BuyProductImage>()
                        .Where(pi => pi.ProductId == product.Id)
                        .ToList();

                    product.NonMappedImages = productImages.Select(pi => new Image(pi.Url)).ToList();
                    Debug.WriteLine($"[Repository] Loaded {product.NonMappedImages.Count} images for product {product.Id}");
                }
                else
                {
                    Debug.WriteLine($"[Repository] WARNING: Product with ID {item.ProductId} not found!");
                }
            }

            Debug.WriteLine($"[Repository] Returning {basketItems.Count} fully loaded basket items");
            return basketItems;
        }

        public void AddItemToBasket(int basketId, int productId, int quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            if (productId < 0)
            {
                throw new ArgumentException("Product ID cannot be negative");
            }

            // Get the product
            var product = _context.BuyProducts.Find(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            // Check if the item already exists in the basket
            var existingItem = _context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (existingItem != null)
            {
                // Update existing item quantity
                existingItem.Quantity += quantity;
                _context.BasketItems.Update(existingItem);
            }
            else
            {
                // Create new basket item - don't assign Product directly since it's NotMapped
                var basketItem = new BasketItem
                {
                    BasketId = basketId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price
                };
                _context.BasketItems.Add(basketItem);
            }

            _context.SaveChanges();
        }

        public void UpdateProductQuantity(int basketId, int productId, int quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            if (productId < 0)
            {
                throw new ArgumentException("Product ID cannot be negative");
            }

            // Find the basket item
            var basketItem = _context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (basketItem == null)
            {
                throw new Exception("Basket item not found");
            }

            if (quantity == 0)
            {
                // Remove the item if quantity is 0
                _context.BasketItems.Remove(basketItem);
            }
            else
            {
                // Update quantity
                basketItem.Quantity = quantity;
                _context.BasketItems.Update(basketItem);
            }

            _context.SaveChanges();
        }

        public void ClearBasket(int basketId)
        {
            if (basketId < NOBASKET)
            {
                throw new ArgumentException("Basket ID cannot be negative");
            }

            var basketItems = _context.BasketItems
                .Where(bi => bi.BasketId == basketId);

            _context.BasketItems.RemoveRange(basketItems);
            _context.SaveChanges();
        }

        public bool RemoveProductFromBasket(int basketId, int productId)
        {
            if (productId < 0)
            {
                throw new ArgumentException("Product ID cannot be negative");
            }

            // Find the basket item
            var basketItem = _context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (basketItem == null)
            {
                return false; // Item not found
            }

            // Remove the item
            _context.BasketItems.Remove(basketItem);
            _context.SaveChanges();

            return true;
        }

        public void UpdateItemQuantityByProductId(int basketId, int productId, int quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            if (productId < 0)
            {
                throw new ArgumentException("Product ID cannot be negative");
            }

            // Find the basket item
            var basketItem = _context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (basketItem == null)
            {
                throw new Exception("Basket item not found");
            }

            if (quantity == 0)
            {
                // Remove the item if quantity is 0
                _context.BasketItems.Remove(basketItem);
            }
            else
            {
                // Update quantity
                basketItem.Quantity = quantity;
                _context.BasketItems.Update(basketItem);
            }

            _context.SaveChanges();
        }
    }
}