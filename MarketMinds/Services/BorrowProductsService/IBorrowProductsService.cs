using System.Collections.Generic;
using MarketMinds.Shared.Models;

namespace MarketMinds.Services.BorrowProductsService
{
    /// <summary>
    /// Interface for the borrow products service.
    /// </summary>
    public interface IBorrowProductsService
    {
        /// <summary>
        /// Creates a new product listing.
        /// </summary>
        /// <param name="product">The product to add.</param>
        void CreateListing(Product product);

        /// <summary>
        /// Deletes an existing product listing.
        /// </summary>
        /// <param name="product">The product to delete.</param>
        void DeleteListing(Product product);

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>A list of all products.</returns>
        List<Product> GetProducts();

        /// <summary>
        /// Gets a product by ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>The product with the specified ID.</returns>
        Product GetProductById(int id);
    }
}
