// using DomainLayer.Domain; // Removed incorrect using
using server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketMinds.Repositories.AuctionProductsRepository
{
    public interface IAuctionProductsRepository
    {
        /// <summary>
        /// Retrieves all auction products from the repository.
        /// </summary>
        /// <returns>A list of all auction products.</returns>
        new List<AuctionProduct> GetProducts();
        AuctionProduct GetProductByID(int id);
        void AddProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(Product product);
    }
} 