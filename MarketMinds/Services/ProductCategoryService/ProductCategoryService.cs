using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;

namespace MarketMinds.Services.ProductCategoryService
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository _repository;

        public ProductCategoryService(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public List<ProductCategory> GetAllProductCategories()
        {
            return _repository.GetAllProductCategories();
        }

        public ProductCategory CreateProductCategory(string name, string description)
        {
            return _repository.CreateProductCategory(name, description);
        }

        public void DeleteProductCategory(string name)
        {
            _repository.DeleteProductCategory(name);
        }
    }

    public class ProductCategoryRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string? Description { get; set; }
    }
}