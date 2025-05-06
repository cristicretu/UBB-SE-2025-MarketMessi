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
            return productCategoryRepository.GetAllProductCategories();
        }

        public Category CreateProductCategory(string name, string description)
        {
            return productCategoryRepository.CreateProductCategory(name, description);
        }

        public void DeleteProductCategory(string name)
        {
            productCategoryRepository.DeleteProductCategory(name);
        }
    }

    public class ProductCategoryRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string? Description { get; set; }
    }
}