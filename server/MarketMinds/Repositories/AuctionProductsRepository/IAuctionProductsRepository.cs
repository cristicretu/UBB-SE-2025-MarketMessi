// using DomainLayer.Domain; // Removed incorrect using
using server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketMinds.Repositories.AuctionProductsRepository
{
    public interface IAuctionProductsRepository
    {
        List<Product> GetProducts();
        AuctionProduct GetProductByID(int id);
        void AddProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(Product product);
    }
} 