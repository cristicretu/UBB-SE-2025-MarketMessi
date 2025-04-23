using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.ProductCategoryService
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public ProductCategoryService(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }

            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public List<ProductCategory> GetAllProductCategories()
        {
            var response = httpClient.GetAsync("ProductCategory").Result;
            response.EnsureSuccessStatusCode();
            var responseJson = response.Content.ReadAsStringAsync().Result;

            var clientCategories = new List<ProductCategory>();
            var responseJsonArray = JsonNode.Parse(responseJson)?.AsArray();

            if (responseJsonArray != null)
            {
                foreach (var responseJsonItem in responseJsonArray)
                {
                    var id = responseJsonItem["id"]?.GetValue<int>() ?? 0;
                    var name = responseJsonItem["name"]?.GetValue<string>() ?? string.Empty;
                    var description = responseJsonItem["description"]?.GetValue<string>() ?? string.Empty;

                    clientCategories.Add(new ProductCategory(id, name, description));
                }
            }

            return clientCategories;
        }

        public ProductCategory CreateProductCategory(string displayTitle, string description)
        {
            var requestContent = new StringContent(
                $"{{\"displayTitle\":\"{displayTitle}\",\"description\":\"{description}\"}}",
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
            var name = jsonObject["name"]?.GetValue<string>() ?? string.Empty;
            var categoryDescription = jsonObject["description"]?.GetValue<string>() ?? string.Empty;

            return new ProductCategory(id, name, categoryDescription);
        }

        public void DeleteProductCategory(string displayTitle)
        {
            var response = httpClient.DeleteAsync($"ProductCategory/{displayTitle}").Result;
            response.EnsureSuccessStatusCode();
        }
    }

    public class ProductCategoryRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string? Description { get; set; }
    }
}