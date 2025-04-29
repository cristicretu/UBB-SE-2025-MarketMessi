using System;
using System.Collections.Generic;
using System.Linq;
using MarketMinds.Shared.Models;
using MarketMinds.Repositories.BasketRepository;

namespace MarketMinds.Test.Services.BasketServiceTest
{
    public class BasketRepositoryMock : IBasketRepository
    {
        private readonly Dictionary<int, Basket> _baskets = new();
        private readonly Dictionary<int, List<BasketItem>> _basketItems = new();
        
        private int NEXT_BASKET_ID = 1;
        private int NEXT_ITEM_ID = 1;

        private int _addItemCount;
        private int _removeItemCount;
        private int _updateItemCount;
        private int _clearBasketCount;

        private bool _throwUpdateException;
        private bool _throwRemoveException;
        private bool _throwGetBasketException;

        public Basket GetBasketByUserId(int userId)
        {
            if (_throwGetBasketException)
                throw new Exception("Test exception for GetBasketByUserId");

            ValidateUserId(userId);

            if (_baskets.ContainsKey(userId))
                return _baskets[userId];

            var basket = new Basket(NEXT_BASKET_ID++);
            _baskets[userId] = basket;
            _basketItems[basket.Id] = new List<BasketItem>();
            return basket;
        }

        public List<BasketItem> GetBasketItems(int basketId)
        {
            return _basketItems.ContainsKey(basketId) 
                ? new List<BasketItem>(_basketItems[basketId]) 
                : new List<BasketItem>();
        }

        public void AddItemToBasket(int basketId, int productId, int quantity)
        {
            ValidateQuantity(quantity);
            ValidateProductId(productId);

            EnsureBasketExists(basketId);

            var existingItem = _basketItems[basketId]
                .FirstOrDefault(i => i.Product?.Id == productId);

            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
            }
            else
            {
                var product = CreateTestProduct(productId);
                var item = new BasketItem(NEXT_ITEM_ID++, product, quantity);
                _basketItems[basketId].Add(item);
            }

            _addItemCount++;
        }

        public void RemoveItemByProductId(int basketId, int productId)
        {
            if (_throwRemoveException)
                throw new Exception("Test exception for RemoveItemByProductId");

            if (_basketItems.ContainsKey(basketId))
            {
                _basketItems[basketId].RemoveAll(i => i.Product?.Id == productId);
            }

            _removeItemCount++;
        }

        public void UpdateItemQuantityByProductId(int basketId, int productId, int quantity)
        {
            if (_throwUpdateException)
                throw new Exception("Test exception for UpdateItemQuantityByProductId");

            ValidateQuantity(quantity);

            if (_basketItems.ContainsKey(basketId))
            {
                var item = _basketItems[basketId]
                    .FirstOrDefault(i => i.Product?.Id == productId);
                
                if (item != null)
                {
                    item.Quantity = quantity;
                    _updateItemCount++;
                }
            }
            else
            {
                _updateItemCount++;
            }
        }

        public void ClearBasket(int basketId)
        {
            if (_basketItems.ContainsKey(basketId))
            {
                _basketItems[basketId].Clear();
            }

            _clearBasketCount++;
        }

        public int GetAddItemCount() => _addItemCount;
        public int GetRemoveItemCount() => _removeItemCount;
        public int GetUpdateItemCount() => _updateItemCount;
        public int GetClearBasketCount() => _clearBasketCount;

        public void SetupInvalidItemQuantity(int basketId)
        {
            EnsureBasketExists(basketId);

            var invalidProduct = CreateTestProduct(1, "Invalid Quantity Product", "Product with invalid quantity");
            var invalidItem = new BasketItem(NEXT_ITEM_ID++, invalidProduct, 0);

            _basketItems[basketId].Add(invalidItem);
        }

        public void SetupInvalidItemPrice(int basketId)
        {
            EnsureBasketExists(basketId);

            var invalidPriceProduct = new TestProductWithNoPrice
            {
                Id = 2,
                Title = "Invalid Price Product",
                Description = "Product with invalid price",
                Seller = new User(1, "Test Seller", "seller@test.com"),
                Condition = new Condition(1, "New", "Brand new item"),
                Category = new Category(1, "Electronics", "Electronic devices"),
                Tags = new List<ProductTag>(),
                Images = new List<Image>()
            };
            var invalidItem = new BasketItem(NEXT_ITEM_ID++, invalidPriceProduct, 1);
            _basketItems[basketId].Add(invalidItem);
        }

        public void SetupValidBasket(int basketId)
        {
            if (!_basketItems.ContainsKey(basketId))
                _basketItems[basketId] = new List<BasketItem>();
            else
                _basketItems[basketId].Clear();

            var product1 = CreateTestProduct(3, "Valid Product", "Product with valid attributes", 100);
            var product2 = CreateTestProduct(4, "Second Product", "Another product with valid attributes", 50);

            _basketItems[basketId].Add(new BasketItem(NEXT_ITEM_ID++, product1, 1));
            _basketItems[basketId].Add(new BasketItem(NEXT_ITEM_ID++, product2, 1));
        }

        public void SetupUpdateQuantityException() => _throwUpdateException = true;
        public void SetupRemoveItemException() => _throwRemoveException = true;
        public void SetupGetBasketException() => _throwGetBasketException = true;

        private void ValidateUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID cannot be negative or zero");
        }

        private void ValidateQuantity(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative");
        }

        private void ValidateProductId(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("Product ID cannot be negative or zero");
        }

        private void EnsureBasketExists(int basketId)
        {
            if (!_basketItems.ContainsKey(basketId))
                _basketItems[basketId] = new List<BasketItem>();
        }

        private BuyProduct CreateTestProduct(int id, string title = "Test Product", string description = "Test Description", int price = 100)
        {
            return new BuyProduct(
                id,
                title,
                description,
                new User(1, "Test Seller", "seller@test.com"),
                new Condition(1, "New", "Brand new item"),
                new Category(1, "Electronics", "Electronic devices"),
                new List<ProductTag>(),
                new List<Image>(),
                price
            );
        }

        private class TestProductWithNoPrice : Product
        {
            // Intentionally no price field
        }
    }
}
