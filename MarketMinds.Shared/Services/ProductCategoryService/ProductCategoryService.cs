using System;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Shared.Services.ProductCategoryService
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository productCategoryRepository;

        public ProductCategoryService(IProductCategoryRepository repository)
        {
            productCategoryRepository = repository;
        }

        public List<Category> GetAllProductCategories()
        {
            var categories = productCategoryRepository.GetAllProductCategories();
            if (categories == null)
            {
                throw new InvalidOperationException("Failed to retrieve product categories.");
            }
            return categories;
        }

        public Category CreateProductCategory(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
            }

            if (name.Length > 100)
            {
                throw new ArgumentException("Category name cannot exceed 100 characters.", nameof(name));
            }

            if (description != null && description.Length > 500)
            {
                throw new ArgumentException("Category description cannot exceed 500 characters.", nameof(description));
            }

            var category = productCategoryRepository.CreateProductCategory(name, description);
            if (category == null)
            {
                throw new InvalidOperationException("Failed to create product category.");
            }

            return category;
        }

        public void DeleteProductCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
            }

            productCategoryRepository.DeleteProductCategory(name);
        }
    }

    public class ProductCategoryRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string? Description { get; set; }
    }
}