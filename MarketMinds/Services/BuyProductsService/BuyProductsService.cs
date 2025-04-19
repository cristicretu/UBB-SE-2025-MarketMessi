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

        public void CreateListing(Product product)
        {
            if (!(product is BuyProduct buyProduct))
            {
                throw new ArgumentException("Product must be a BuyProduct.", nameof(product));
            }
            var response = httpClient.PostAsJsonAsync("buyproducts", buyProduct).Result;
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