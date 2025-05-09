using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.ProxyRepository
{
    public class BuyProductsProxyRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public BuyProductsProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        // Raw data access methods
        public string GetProductsRaw()
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var response = httpClient.GetAsync("buyproducts").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public string CreateListingRaw(object productToSend)
        {
            var response = httpClient.PostAsJsonAsync("buyproducts", productToSend).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public void DeleteListingRaw(int id)
        {
            var response = httpClient.DeleteAsync($"buyproducts/{id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public string GetProductByIdRaw(int id)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var response = httpClient.GetAsync($"buyproducts/{id}").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        // Legacy implementations that delegate to service layer
        public List<Models.Product> GetProducts()
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void CreateListing(Models.BuyProduct product)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void DeleteListing(Models.Product product)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public Models.BuyProduct GetProductById(int id)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }
    }
}
