using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.Domain;

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
        /// Updates an existing product listing.
        /// </summary>
        /// <param name="product">The product to update.</param>
        void UpdateListing(Product product);

        /// <summary>
        /// Deletes an existing product listing.
        /// </summary>
        /// <param name="product">The product to delete.</param>
        void DeleteListing(Product product);

        /// <summary>
        /// Borrows a product.
        /// </summary>
        /// <param name="product">The product to borrow.</param>
        /// <param name="borrower">The borrower.</param>
        /// <param name="startDate">The start date of the borrowing period.</param>
        /// <param name="endDate">The end date of the borrowing period.</param>
        void BorrowProduct(BorrowProduct product, User borrower, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the time left for a borrowing period.
        /// </summary>
        /// <param name="product">The product to check.</param>
        /// <returns>The time left for the borrowing period.</returns>
        string GetTimeLeft(BorrowProduct product);

        /// <summary>
        /// Checks if a borrowing period has ended.
        /// </summary>
        /// <param name="product">The product to check.</param>
        /// <returns>True if the borrowing period has ended, false otherwise.</returns>
        bool IsBorrowPeriodEnded(BorrowProduct product);

        /// <summary>
        /// Gets a list of all products.
        /// </summary>
        /// <returns>A list of all products.</returns>
        List<Product> GetProducts();

        /// <summary>
        /// Gets a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <returns>The product with the specified ID.</returns>
        Product GetProductById(int id);

        /// <summary>
        /// Sorts and filters a list of products.
        /// </summary>
        /// <param name="sortOption">The sort option.</param>
        /// <param name="filterOption">The filter option.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <returns>A sorted and filtered list of products.</returns>
        Task<IEnumerable<Product>> SortAndFilter(string sortOption, string filterOption, string filterValue);
    }
}
