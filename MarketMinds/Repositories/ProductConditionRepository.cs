using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;

namespace MarketMinds.Repositories
{
    public class ProductConditionRepository : IProductConditionRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public ProductConditionRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public List<ProductCondition> GetAllProductConditions()
        {
            var response = httpClient.GetAsync("ProductCondition").Result;
            response.EnsureSuccessStatusCode();
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var clientConditions = new List<ProductCondition>();
            var responseJsonArray = JsonNode.Parse(responseJson)?.AsArray();

            if (responseJsonArray != null)
            {
                foreach (var responseJsonItem in responseJsonArray)
                {
                    var id = responseJsonItem["id"]?.GetValue<int>() ?? 0;
                    var name = responseJsonItem["name"]?.GetValue<string>() ?? string.Empty;
                    var description = responseJsonItem["description"]?.GetValue<string>() ?? string.Empty;
                    clientConditions.Add(new ProductCondition(id, name, description));
                }
            }
            return clientConditions;
        }

        public ProductCondition CreateProductCondition(string displayTitle, string description)
        {
            var requestContent = new StringContent(
                $"{{\"displayTitle\":\"{displayTitle}\",\"description\":\"{description}\"}}",
                System.Text.Encoding.UTF8,
                "application/json");
            var response = httpClient.PostAsync("ProductCondition", requestContent).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var jsonObject = JsonNode.Parse(json);

            if (jsonObject == null)
            {
                throw new InvalidOperationException("Failed to parse the server response.");
            }

            var id = jsonObject["id"]?.GetValue<int>() ?? 0;
            var name = jsonObject["name"]?.GetValue<string>() ?? string.Empty;
            var conditionDescription = jsonObject["description"]?.GetValue<string>() ?? string.Empty;
            return new ProductCondition(id, name, conditionDescription);
        }

        public void DeleteProductCondition(string displayTitle)
        {
            var response = httpClient.DeleteAsync($"ProductCondition/{displayTitle}").Result;
            response.EnsureSuccessStatusCode();
        }
    }

    public class ProductConditionRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}