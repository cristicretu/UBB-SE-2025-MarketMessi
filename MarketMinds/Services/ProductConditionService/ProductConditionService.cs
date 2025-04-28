using System.Net.Http;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.ProductConditionService
{
    public class ProductConditionService : IProductConditionService
    {
        private readonly IProductConditionRepository productConditionRepository;

        public ProductConditionService(IProductConditionRepository repository)
        {
            productConditionRepository = repository;
        }

        public List<Condition> GetAllProductConditions()
        {
            return productConditionRepository.GetAllProductConditions();
        }

        public Condition CreateProductCondition(string displayTitle, string description)
        {
            return productConditionRepository.CreateProductCondition(displayTitle, description);
        }

        public void DeleteProductCondition(string displayTitle)
        {
            productConditionRepository.DeleteProductCondition(displayTitle);
        }
    }

    public class ProductConditionRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}