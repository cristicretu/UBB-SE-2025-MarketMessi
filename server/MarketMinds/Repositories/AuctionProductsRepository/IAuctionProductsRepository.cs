using DomainLayer.Domain; // Assuming Product model is here
using System.Collections.Generic;

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