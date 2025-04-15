using Microsoft.Extensions.Configuration;

namespace MarketMinds.Services.ApiService
{
    public static class ApiConfig
    {
        private static string base_url = "http://localhost:5000"; // Default value

        // Initialize configuration
        public static void Initialize(IConfiguration configuration)
        {
            base_url = configuration["ApiSettings:BaseUrl"] ?? base_url;
        }

        // Base URL for the API - loaded from configuration
        public static string BaseUrl => base_url;
        // API endpoints
        public static class Endpoints
        {
            public static string Messi => $"{BaseUrl}/messi";
            public static string Weather => $"{BaseUrl}/weatherforecast";
            // Add more endpoints as needed
        }
    }
}