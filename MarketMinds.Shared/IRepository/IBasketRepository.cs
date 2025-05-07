using System.Net.Http;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Interface for BasketRepository to manage basket operations.
    /// </summary>
    public interface IBasketRepository
    {
        /// <summary>
        /// Retrieves the user's basket, or creates one if it doesn't exist.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user's basket.</returns>
        Basket GetBasketByUserId(int userId);

        /// <summary>
        /// Removes an item from the basket by product ID.
        /// </summary>
        /// <param name="basketId">The ID of the basket.</param>
        /// <param name="productId">The ID of the product to remove.</param>
        void RemoveItemByProductId(int basketId, int productId);

        /// <summary>
        /// Retrieves the items in a basket.
        /// </summary>
        /// <param name="basketId">The ID of the basket.</param>
        /// <returns>A list of basket items.</returns>
        List<BasketItem> GetBasketItems(int basketId);

        /// <summary>
        /// Adds an item to the basket.
        /// </summary>
        /// <param name="basketId">The ID of the basket.</param>
        /// <param name="productId">The ID of the product to add.</param>
        /// <param name="quantity">The quantity of the product to add.</param>
        void AddItemToBasket(int basketId, int productId, int quantity);

        /// <summary>
        /// Updates the quantity of an item in the basket.
        /// </summary>
        /// <param name="basketId">The ID of the basket.</param>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="quantity">The new quantity of the product.</param>
        void UpdateItemQuantityByProductId(int basketId, int productId, int quantity);

        /// <summary>
        /// Removes all items from a basket.
        /// </summary>
        /// <param name="basketId">The ID of the basket to clear.</param>
        void ClearBasket(int basketId);

        /// <summary>
        /// Makes an HTTP call to add a product to a user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to add.</param>
        /// <param name="quantity">The quantity of the product to add.</param>
        /// <returns>The HTTP response message.</returns>
        HttpResponseMessage AddProductToBasketRaw(int userId, int productId, int quantity);

        /// <summary>
        /// Makes an HTTP call to get a user's basket and returns the raw JSON response.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The raw JSON response as a string.</returns>
        string GetBasketByUserRaw(int userId);

        /// <summary>
        /// Makes an HTTP call to remove a product from a user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to remove.</param>
        /// <returns>The HTTP response message.</returns>
        HttpResponseMessage RemoveProductFromBasketRaw(int userId, int productId);

        /// <summary>
        /// Makes an HTTP call to update the quantity of a product in a user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="quantity">The new quantity of the product.</param>
        /// <returns>The HTTP response message.</returns>
        HttpResponseMessage UpdateProductQuantityRaw(int userId, int productId, int quantity);

        /// <summary>
        /// Makes an HTTP call to clear a user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The HTTP response message.</returns>
        HttpResponseMessage ClearBasketRaw(int userId);

        /// <summary>
        /// Makes an HTTP call to validate a basket before checkout.
        /// </summary>
        /// <param name="basketId">The ID of the basket to validate.</param>
        /// <returns>The raw JSON response as a string.</returns>
        string ValidateBasketBeforeCheckOutRaw(int basketId);

        /// <summary>
        /// Makes an HTTP call to apply a promo code to a basket.
        /// </summary>
        /// <param name="basketId">The ID of the basket.</param>
        /// <param name="code">The promo code to apply.</param>
        /// <returns>The HTTP response message.</returns>
        HttpResponseMessage ApplyPromoCodeRaw(int basketId, string code);

        /// <summary>
        /// Makes an HTTP call to get the discount for a promo code.
        /// </summary>
        /// <param name="code">The promo code.</param>
        /// <returns>The raw JSON response as a string.</returns>
        string GetPromoCodeDiscountRaw(string code);

        /// <summary>
        /// Makes an HTTP call to calculate the totals for a basket.
        /// </summary>
        /// <param name="basketId">The ID of the basket.</param>
        /// <param name="promoCode">The promo code to apply (optional).</param>
        /// <returns>The raw JSON response as a string.</returns>
        string CalculateBasketTotalsRaw(int basketId, string promoCode);

        /// <summary>
        /// Makes an HTTP call to decrease the quantity of a product in a user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The HTTP response message.</returns>
        HttpResponseMessage DecreaseProductQuantityRaw(int userId, int productId);

        /// <summary>
        /// Makes an HTTP call to increase the quantity of a product in a user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The HTTP response message.</returns>
        HttpResponseMessage IncreaseProductQuantityRaw(int userId, int productId);

        /// <summary>
        /// Makes an HTTP call to checkout a basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="basketId">The ID of the basket.</param>
        /// <param name="requestData">The request data to send.</param>
        /// <returns>The HTTP response message.</returns>
        Task<HttpResponseMessage> CheckoutBasketRaw(int userId, int basketId, object requestData);
    }
}