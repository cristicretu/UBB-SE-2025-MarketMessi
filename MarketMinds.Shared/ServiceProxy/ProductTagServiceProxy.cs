using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.ServiceProxy
{
    public class ProductTagServiceProxy : IProductTagRepository
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public ProductTagServiceProxy(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }

            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }

            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
            Console.WriteLine($"ProductTag Repository initialized with base address: {httpClient.BaseAddress}");

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public List<ProductTag> GetAllProductTags()
        {
            try
            {
                var response = httpClient.GetAsync("ProductTag").Result;
                response.EnsureSuccessStatusCode();
                var responseJson = response.Content.ReadAsStringAsync().Result;
                var tags = JsonSerializer.Deserialize<List<ProductTag>>(responseJson, jsonOptions);
                return tags ?? new List<ProductTag>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all product tags: {ex.Message}");
                return new List<ProductTag>();
            }
        }

        public ProductTag CreateProductTag(string displayTitle)
        {
            try
            {
                var requestContent = new StringContent(
                    $"{{\"displayTitle\":\"{displayTitle}\"}}",
                    System.Text.Encoding.UTF8,
                    "application/json");
                var response = httpClient.PostAsync("ProductTag", requestContent).Result;
                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().Result;
                var tag = JsonSerializer.Deserialize<ProductTag>(json, jsonOptions);
                if (tag == null)
                {
                    throw new InvalidOperationException("Failed to parse the server response.");
                }
                return tag;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product tag: {ex.Message}");
                throw;
            }
        }

        public void DeleteProductTag(string displayTitle)
        {
            try
            {
                var response = httpClient.DeleteAsync($"ProductTag/{displayTitle}").Result;
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product tag: {ex.Message}");
                throw;
            }
        }
    }
}