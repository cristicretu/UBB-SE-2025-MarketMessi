using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Repository;

namespace MarketMinds.Services.BuyProductsService
{
    public class BuyProductsService : IBuyProductsService
    {
        private readonly BuyProductsRepository buyProductsRepository;

        public BuyProductsService(BuyProductsRepository buyProductsRepository)
        {
            this.buyProductsRepository = buyProductsRepository;
        }

        public List<Product> GetProducts()
        {
            return buyProductsRepository.GetProducts();
        }

        public void CreateListing(BuyProduct product)
        {
            buyProductsRepository.CreateListing(product);
        }

        public void DeleteListing(Product product)
        {
            buyProductsRepository.DeleteListing(product);
        }

        public BuyProduct GetProductById(int id)
        {
            return buyProductsRepository.GetProductById(id);
        }
    }
}