using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using DomainLayer.Domain;
using Microsoft.Extensions.Configuration;
using MarketMinds.Services.ProductTagService;

namespace MarketMinds.Services.BuyProductsService
{
    public class BuyProductsService : ProductService, IBuyProductsService
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public BuyProductsService(IConfiguration configuration) : base(null)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public override List<Product> GetProducts()
        {
            var products = httpClient.GetFromJsonAsync<List<BuyProduct>>("buyproducts").Result;
            return products?.Cast<Product>().ToList() ?? new List<Product>();
        }

        public void CreateListing(Product product)
        {
            if (!(product is BuyProduct buyProduct))
            {
                throw new ArgumentException("Product must be a BuyProduct.", nameof(product));
            }
            var productToSend = new
            {
                buyProduct.Title,
                buyProduct.Description,
                SellerId = buyProduct.Seller?.Id ?? 0,
                ConditionId = buyProduct.Condition?.Id,
                CategoryId = buyProduct.Category?.Id,
                buyProduct.Price,
                Images = buyProduct.Images == null
                       ? new List<object>()
                       : buyProduct.Images.Select(img => new { img.Url }).Cast<object>().ToList()
            };
            var response = httpClient.PostAsJsonAsync("buyproducts", productToSend).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                throw new HttpRequestException($"Failed to create listing. Status: {response.StatusCode}, Error: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
        }

        public void DeleteListing(Product product)
        {
            if (product.Id == 0)
            {
                throw new ArgumentException("Product ID must be set for delete.", nameof(product.Id));
            }
            var response = httpClient.DeleteAsync($"buyproducts/{product.Id}").Result;
            response.EnsureSuccessStatusCode();
        }
    }
}