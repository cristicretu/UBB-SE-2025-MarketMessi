using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using server.DataAccessLayer;
using server.Models;

namespace MarketMinds.Repositories.BasketRepository
{
    public class BasketRepository : IBasketRepository
    {
        private const int INVALID_BASKET_ID = -1;
        private const int INVALID_ITEM_ID = -1;
        private const int DEFAULT_PRICE = 0;
        private const int MINIMUM_VALID_ID = 0;
        private const int ZERO_QUANTITY = 0;
        
        private readonly ApplicationDbContext _context;

        public BasketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Basket GetBasketByUserId(int userId)
        {
            var basketEntity = _context.Baskets
                .FirstOrDefault(b => b.BuyerId == userId);

            if (basketEntity == null)
            {
                basketEntity = new Basket
                {
                    BuyerId = userId
                };
                _context.Baskets.Add(basketEntity);
                _context.SaveChanges();
            }

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

            var basketItems = _context.BasketItems
                .Where(bi => bi.BasketId == basketId)
                .ToList();

            Debug.WriteLine($"[Repository] Found {basketItems.Count} basket items in database");

            foreach (var item in basketItems)
            {
                Debug.WriteLine($"[Repository] Loading product details for item {item.Id}, productId: {item.ProductId}");

                var product = _context.BuyProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Id == item.ProductId);

                if (product != null)
                {
                    Debug.WriteLine($"[Repository] Found product: {product.Id} - {product.Title}");
                    item.Product = product;

                    if (item.ProductId != product.Id)
                    {
                        Debug.WriteLine($"[Repository] WARNING: ProductId mismatch! Setting ProductId={product.Id} for item {item.Id}");
                        item.ProductId = product.Id;
                    }

                    var productTagIds = _context.Set<BuyProductProductTag>()
                        .Where(pt => pt.ProductId == product.Id)
                        .Select(pt => pt.TagId)
                        .ToList();

                    var tags = _context.Set<ProductTag>()
                        .Where(t => productTagIds.Contains(t.Id))
                        .ToList();

                    product.Tags = tags;
                    Debug.WriteLine($"[Repository] Loaded {tags.Count} tags for product {product.Id}");

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
            ValidateQuantity(quantity);
            ValidateProductId(productId);

            var product = _context.BuyProducts.Find(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            var existingItem = _context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                _context.BasketItems.Update(existingItem);
            }
            else
            {
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
            UpdateItemQuantityByProductId(basketId, productId, quantity);
        }

        public void ClearBasket(int basketId)
        {
            if (basketId < INVALID_BASKET_ID)
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
            ValidateProductId(productId);

            var basketItem = _context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (basketItem == null)
            {
                return false;
            }

            _context.BasketItems.Remove(basketItem);
            _context.SaveChanges();

            return true;
        }

        public void UpdateItemQuantityByProductId(int basketId, int productId, int quantity)
        {
            ValidateQuantity(quantity);
            ValidateProductId(productId);

            var basketItem = _context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (basketItem == null)
            {
                throw new Exception("Basket item not found");
            }

            if (quantity == ZERO_QUANTITY)
            {
                _context.BasketItems.Remove(basketItem);
            }
            else
            {
                basketItem.Quantity = quantity;
                _context.BasketItems.Update(basketItem);
            }

            _context.SaveChanges();
        }

        private void ValidateQuantity(int quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }
        }

        private void ValidateProductId(int productId)
        {
            if (productId < 0)
            {
                throw new ArgumentException("Product ID cannot be negative");
            }
        }
    }
}
