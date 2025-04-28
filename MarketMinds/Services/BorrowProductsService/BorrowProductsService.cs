using System.Net.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Repository;

namespace MarketMinds.Services.BorrowProductsService
{
    public class BorrowProductsService : IBorrowProductsService
    {
        private readonly BorrowProductsRepository borrowProductsRepository;

        public BorrowProductsService(BorrowProductsRepository borrowProductsRepository)
        {
           this.borrowProductsRepository = borrowProductsRepository;
        }

        public void CreateListing(Product product)
        {
            borrowProductsRepository.CreateListing(product);
        }

        public void DeleteListing(Product product)
        {
            borrowProductsRepository.DeleteListing(product);
        }

        public List<Product> GetProducts()
        {
           return borrowProductsRepository.GetProducts();
        }

        public Product GetProductById(int id)
        {
            return borrowProductsRepository.GetProductById(id);
        }
    }
}