using System.Collections.Generic;
using Server.Models;

namespace MarketMinds.Repositories.ProductConditionRepository
{
    /// <summary>
    /// Interface for ProductConditionRepository to manage product condition operations.
    /// </summary>
    public interface IProductConditionRepository
    {
        /// <summary>
        /// Returns all the product conditions.
        /// </summary>
        /// <returns>A list of all product conditions.</returns>
        List<Condition> GetAllProductConditions();

        /// <summary>
        /// Creates a new product condition.
        /// </summary>
        /// <param name="displayTitle">The display title of the product condition.</param>
        /// <param name="description">The description of the product condition.</param>
        /// <returns>The created product condition.</returns>
        Condition CreateProductCondition(string displayTitle, string description);

        /// <summary>
        /// Deletes a product condition by its display title.
        /// </summary>
        /// <param name="displayTitle">The display title of the product condition to delete.</param>
        void DeleteProductCondition(string displayTitle);
    }
}

