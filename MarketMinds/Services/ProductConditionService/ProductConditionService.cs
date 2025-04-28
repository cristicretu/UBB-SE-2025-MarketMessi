using MarketMinds.Shared.IRepository;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.ProductConditionService
{
    public class ProductConditionService : IProductConditionService
    {
        private readonly IProductConditionRepository _repository;

        public ProductConditionService(IProductConditionRepository repository)
        {
            _repository = repository;
        }

        public List<Condition> GetAllProductConditions()
        {
            return _repository.GetAllProductConditions();
        }

        public Condition CreateProductCondition(string displayTitle, string description)
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