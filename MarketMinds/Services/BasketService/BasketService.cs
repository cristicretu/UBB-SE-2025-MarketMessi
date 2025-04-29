using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.ServiceProxy;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.BasketService
{
    public class BasketService : IBasketService
    {
        public const int MAXIMUM_QUANTITY_PER_ITEM = 10;
        private const int MINIMUM_USER_ID = 0;
        private const int MINIMUM_ITEM_ID = 0;
        private const int MINIMUM_BASKET_ID = 0;
        private const double MINIMUM_DISCOUNT = 0;
        private const int MINIMUM_QUANTITY = 0;
        private const int DEFAULT_QUANTITY = 1;
        private const int INVALID_USER_ID = -1;
        private const int INVALID_BASKET_ID = -1;

        private readonly BasketServiceProxy basketRepository;

        // Constructor with configuration
        public BasketService(BasketServiceProxy basketRepository)
        {
            this.basketRepository = basketRepository;
        }

        public void AddProductToBasket(int userId, int productId, int quantity)
        {
            basketRepository.AddProductToBasket(userId, productId, quantity);
        }

        public Basket GetBasketByUser(User user)
        {
            return basketRepository.GetBasketByUser(user);
        }

        public void RemoveProductFromBasket(int userId, int productId)
        {
            basketRepository.RemoveProductFromBasket(userId, productId);
        }

        public void UpdateProductQuantity(int userId, int productId, int quantity)
        {
            basketRepository.UpdateProductQuantity(userId, productId, quantity);
        }

        public bool ValidateQuantityInput(string quantityText, out int quantity)
        {
            return basketRepository.ValidateQuantityInput(quantityText, out quantity);
        }

        public int GetLimitedQuantity(int quantity)
        {
            return Math.Min(quantity, MAXIMUM_QUANTITY_PER_ITEM);
        }

        public void ClearBasket(int userId)
        {
            basketRepository.ClearBasket(userId);
        }

        public bool ValidateBasketBeforeCheckOut(int basketId)
        {
            return basketRepository.ValidateBasketBeforeCheckOut(basketId);
        }

        public void ApplyPromoCode(int basketId, string code)
        {
            basketRepository.ApplyPromoCode(basketId, code);
        }

        // Add a new method to get the discount for a promo code
        public double GetPromoCodeDiscount(string code, double subtotal)
        {
            return basketRepository.GetPromoCodeDiscount(code, subtotal);
        }

        // Add a new method to calculate basket totals
        public BasketTotals CalculateBasketTotals(int basketId, string promoCode)
        {
            return basketRepository.CalculateBasketTotals(basketId, promoCode);
        }

        public void DecreaseProductQuantity(int userId, int productId)
        {
            basketRepository.DecreaseProductQuantity(userId, productId);
        }

        public void IncreaseProductQuantity(int userId, int productId)
        {
            basketRepository.IncreaseProductQuantity(userId, productId);
        }

        public async Task<bool> CheckoutBasketAsync(int userId, int basketId, double discountAmount = 0, double totalAmount = 0)
        {
            return await basketRepository.CheckoutBasketAsync(userId, basketId, discountAmount, totalAmount);
        }
    }
}