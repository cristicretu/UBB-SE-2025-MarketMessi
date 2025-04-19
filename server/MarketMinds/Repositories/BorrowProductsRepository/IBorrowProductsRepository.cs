using System.Collections.Generic;
using server.Models;

namespace MarketMinds.Repositories.BorrowProductsRepository
{
    /// <summary>
    /// Interface for managing borrow products in the repository.
    /// </summary>
    public interface IBorrowProductsRepository
    {
        /// <summary>
        /// Adds a new borrow product to the repository.
        /// </summary>
        /// <param name="product">The product to add.</param>
        void AddProduct(BorrowProduct product);

        /// <summary>
        /// Deletes a borrow product from the repository.
        /// </summary>
        /// <param name="product">The product to delete.</param>
        void DeleteProduct(BorrowProduct product);

        /// <summary>
        /// Retrieves all borrow products from the repository.
        /// </summary>
        /// <returns>A list of all borrow products.</returns>
        List<BorrowProduct> GetProducts();

        /// <summary>
        /// Retrieves a borrow product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The product with the given ID.</returns>
        BorrowProduct GetProductByID(int productId);

        /// <summary>
        /// Updates an existing borrow product in the repository.
        /// </summary>
        /// <param name="product">The product with updated information.</param>
        void UpdateProduct(BorrowProduct product);
    }
} 