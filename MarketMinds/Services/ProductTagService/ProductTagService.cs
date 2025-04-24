using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.ProductTagService
{
    public class ProductTagService : IProductTagService
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public ProductTagService(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public virtual List<ProductTag> GetAllProductTags()
        {
            var response = httpClient.GetAsync("ProductTag").Result;
            response.EnsureSuccessStatusCode();
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var clientTags = new List<ProductTag>();
            var responseJsonArray = JsonNode.Parse(responseJson)?.AsArray();
            if (responseJsonArray != null)
            {
                foreach (var responseJsonItem in responseJsonArray)
                {
                    var id = responseJsonItem["id"]?.GetValue<int>() ?? 0;
                    var title = responseJsonItem["title"]?.GetValue<string>() ?? string.Empty;
                    clientTags.Add(new ProductTag(id, title));
                }
            }
            return clientTags;
        }

        public virtual ProductTag CreateProductTag(string displayTitle)
        {
            var requestContent = new StringContent(
                $"{{\"displayTitle\":\"{displayTitle}\"}}",
                System.Text.Encoding.UTF8,
                "application/json");
            var response = httpClient.PostAsync("ProductTag", requestContent).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var jsonObject = JsonNode.Parse(json);
            if (jsonObject == null)
            {
                throw new InvalidOperationException("Failed to parse the server response.");
            }
            var id = jsonObject["id"]?.GetValue<int>() ?? 0;
            var title = jsonObject["title"]?.GetValue<string>() ?? string.Empty;
            return new ProductTag(id, title);
        }

        public virtual void DeleteProductTag(string displayTitle)
        {
            var response = httpClient.DeleteAsync($"ProductTag/{displayTitle}").Result;
            response.EnsureSuccessStatusCode();
        }
    }
}