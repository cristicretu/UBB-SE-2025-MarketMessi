using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using server.Models;
using System.Data.Common;

namespace MarketMinds.Repositories.BasketRepository
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
    }
}