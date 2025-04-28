using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;

namespace MarketMinds.Services.ProductConditionService
{
    public class ProductConditionService : IProductConditionService
    {
        private readonly IProductConditionRepository _repository;

        public ProductConditionService(IProductConditionRepository repository)
        {
            _repository = repository;
        }

        public List<ProductCondition> GetAllProductConditions()
        {
            return _repository.GetAllProductConditions();
        }

        public ProductCondition CreateProductCondition(string displayTitle, string description)
        {
            return _repository.CreateProductCondition(displayTitle, description);
        }

        public void DeleteProductCondition(string displayTitle)
        {
            _repository.DeleteProductCondition(displayTitle);
        }
    }

    public class ProductConditionRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}