using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MarketMinds.Services.ApiService
{
    public class ApiService
    {
        private readonly HttpClient httpClient;

        public ApiService()
        {
            httpClient = new HttpClient();
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<string> GetStringAsync(string endpoint)
        {
            var response = await httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // Example method to get "ornaldo" message
        public async Task<string> GetMessiMessageAsync()
        {
            return await GetStringAsync(ApiConfig.Endpoints.Messi);
        }

        // Add more API methods as needed
    }
}