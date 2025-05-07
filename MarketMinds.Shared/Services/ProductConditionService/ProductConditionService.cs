using System.Net.Http;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.ProxyRepository;

namespace MarketMinds.Shared.Services.ProductConditionService
{
    public class ProductConditionService : IProductConditionService
    {
        private readonly ProductConditionProxyRepository productConditionRepository;

        public ProductConditionService(IProductConditionRepository repository)
        {
            productConditionRepository = repository as ProductConditionProxyRepository
                ?? throw new ArgumentException("Repository must be of type ProductConditionProxyRepository");
        }

        public List<Condition> GetAllProductConditions()
        {
            var responseJson = productConditionRepository.GetAllProductConditionsRaw();
            var clientConditions = new List<Condition>();
            var responseJsonArray = JsonNode.Parse(responseJson)?.AsArray();

            if (responseJsonArray != null)
            {
                foreach (var responseJsonItem in responseJsonArray)
                {
                    var id = responseJsonItem["id"]?.GetValue<int>() ?? 0;
                    var name = responseJsonItem["name"]?.GetValue<string>() ?? string.Empty;
                    var description = responseJsonItem["description"]?.GetValue<string>() ?? string.Empty;
                    clientConditions.Add(new Condition(id, name, description));
                }
            }
            return clientConditions;
        }

        public Condition CreateProductCondition(string displayTitle, string description)
        {
            var json = productConditionRepository.CreateProductConditionRaw(displayTitle, description);
            var jsonObject = JsonNode.Parse(json);

            if (jsonObject == null)
            {
                throw new InvalidOperationException("Failed to parse the server response.");
            }

            var id = jsonObject["id"]?.GetValue<int>() ?? 0;
            var name = jsonObject["name"]?.GetValue<string>() ?? string.Empty;
            var conditionDescription = jsonObject["description"]?.GetValue<string>() ?? string.Empty;
            return new Condition(id, name, conditionDescription);
        }

        public void DeleteProductCondition(string displayTitle)
        {
            productConditionRepository.DeleteProductConditionRaw(displayTitle);
        }
    }

    public class ProductConditionRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}