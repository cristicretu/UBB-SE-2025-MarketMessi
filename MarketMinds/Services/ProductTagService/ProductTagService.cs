using System;
using System.Linq;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Repositories;

namespace MarketMinds.Services.ProductTagService
{
    public class ProductTagService : IProductTagService
    {
        private readonly ProductTagRepository repository;

        public ProductTagService(IConfiguration configuration)
        {
            repository = new ProductTagRepository(configuration);
        }

        public virtual List<ProductTag> GetAllProductTags()
        {
            try
            {
                var sharedTags = repository.GetAllProductTags();
                return sharedTags.Select(ConvertToDomainTag).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all product tags: {ex.Message}");
                return new List<ProductTag>();
            }
        }

        public virtual ProductTag CreateProductTag(string displayTitle)
        {
            try
            {
                var createdTag = repository.CreateProductTag(displayTitle);
                return ConvertToDomainTag(createdTag);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product tag: {ex.Message}");
                throw;
            }
        }

        public virtual void DeleteProductTag(string displayTitle)
        {
            try
            {
                repository.DeleteProductTag(displayTitle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product tag: {ex.Message}");
                throw;
            }
        }

        // Helper methods to convert between domain and shared models
        private ProductTag ConvertToDomainTag(MarketMinds.Shared.Models.ProductTag sharedTag)
        {
            if (sharedTag == null)
            {
                return null;
            }
            return new ProductTag(sharedTag.Id, sharedTag.Title);
        }
    }
}