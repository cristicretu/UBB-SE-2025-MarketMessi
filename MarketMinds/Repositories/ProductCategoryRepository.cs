using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public ProductCategoryRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public List<Category> GetAllProductCategories()
        {
            var response = httpClient.GetAsync("ProductCategory").Result;
            response.EnsureSuccessStatusCode();
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var categories = new List<Category>();
            var responseJsonArray = JsonNode.Parse(responseJson)?.AsArray();

            if (responseJsonArray != null)
            {
                foreach (var responseJsonItem in responseJsonArray)
                {
                    var id = responseJsonItem["id"]?.GetValue<int>() ?? 0;
                    var name = responseJsonItem["name"]?.GetValue<string>() ?? string.Empty;
                    var description = responseJsonItem["description"]?.GetValue<string>() ?? string.Empty;
                    categories.Add(new Category(id, name, description));
                }
            }
            return categories;
        }

        public Category CreateProductCategory(string name, string description)
        {
            var requestContent = new StringContent(
                $"{{\"DisplayTitle\":\"{name}\",\"Description\":\"{description}\"}}",
                System.Text.Encoding.UTF8,
                "application/json");
            var response = httpClient.PostAsync("ProductCategory", requestContent).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var jsonObject = JsonNode.Parse(json);

            if (jsonObject == null)
            {
                throw new InvalidOperationException("Failed to parse the server response.");
            }

            var id = jsonObject["id"]?.GetValue<int>() ?? 0;
            var categoryName = jsonObject["name"]?.GetValue<string>() ?? string.Empty;
            var categoryDescription = jsonObject["description"]?.GetValue<string>() ?? string.Empty;
            return new Category(categoryName, categoryDescription);
        }

        public void DeleteProductCategory(string name)
        {
            var response = httpClient.DeleteAsync($"ProductCategory/{name}").Result;
            response.EnsureSuccessStatusCode();
        }
    }
}