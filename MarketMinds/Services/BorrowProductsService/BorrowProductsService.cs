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

namespace MarketMinds.Services.BorrowProductsService;

public class BorrowProductsService : ProductService, IBorrowProductsService
{
    private readonly HttpClient httpClient;
    private readonly string apiBaseUrl;

    public BorrowProductsService(IConfiguration configuration) : base(null)
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
        if (!(product is BorrowProduct borrowProduct))
        {
            throw new ArgumentException("Product must be a BorrowProduct.", nameof(product));
        }

        ValidateBorrowProduct(borrowProduct);
        var response = httpClient.PostAsJsonAsync("borrowproducts", borrowProduct).Result;
        response.EnsureSuccessStatusCode();
    }

    public void UpdateListing(Product product)
    {
        if (!(product is BorrowProduct borrowProduct))
        {
            throw new ArgumentException("Product must be a BorrowProduct.", nameof(product));
        }

        if (borrowProduct.Id == 0)
        {
            throw new ArgumentException("Borrow Product ID must be set for update.", nameof(borrowProduct.Id));
        }

        ValidateBorrowProduct(borrowProduct);
        var response = httpClient.PutAsJsonAsync($"borrowproducts/{borrowProduct.Id}", borrowProduct).Result;
        response.EnsureSuccessStatusCode();
    }

    public void DeleteListing(Product product)
    {
        if (product.Id == 0)
        {
            throw new ArgumentException("Borrow Product ID must be set for delete.", nameof(product.Id));
        }
        var response = httpClient.DeleteAsync($"borrowproducts/{product.Id}").Result;
        response.EnsureSuccessStatusCode();
    }

    public void BorrowProduct(BorrowProduct product, User borrower, DateTime startDate, DateTime endDate)
    {
        if (product == null || borrower == null)
        {
            throw new ArgumentNullException("Product and borrower must not be null.");
        }

        if (product.IsBorrowed)
        {
            throw new InvalidOperationException("Product is already borrowed.");
        }

        if (startDate >= endDate)
        {
            throw new ArgumentException("Start date must be before end date.");
        }

        var timeSpan = endDate - startDate;
        var totalCost = timeSpan.Days * product.DailyRate;

        if (borrower.Balance < totalCost)
        {
            throw new InvalidOperationException("Insufficient balance to borrow the product.");
        }

        product.IsBorrowed = true;
        product.StartDate = startDate;
        product.EndDate = endDate;
        borrower.Balance -= totalCost;

        var response = httpClient.PutAsJsonAsync($"borrowproducts/{product.Id}", product).Result;
        response.EnsureSuccessStatusCode();
    }

    public string GetTimeLeft(BorrowProduct product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        if (!product.IsBorrowed)
        {
            return "Not borrowed";
        }

        var timeLeft = product.EndDate - DateTime.Now;
        return timeLeft > TimeSpan.Zero ? timeLeft.ToString(@"dd\:hh\:mm\:ss") : "Borrow period ended";
    }

    public bool IsBorrowPeriodEnded(BorrowProduct product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        return product.IsBorrowed && DateTime.Now >= product.EndDate;
    }

    private void ValidateBorrowProduct(BorrowProduct product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        if (string.IsNullOrWhiteSpace(product.Title))
        {
            throw new ArgumentException("Product title is required.");
        }

        if (string.IsNullOrWhiteSpace(product.Description))
        {
            throw new ArgumentException("Product description is required.");
        }

        if (product.DailyRate <= 0)
        {
            throw new ArgumentException("Daily rate must be greater than 0.");
        }

        if (product.TimeLimit < DateTime.Now)
        {
            throw new ArgumentException("Time limit must be in the future.");
        }
    }

    public override List<Product> GetProducts()
    {
        try
        {
            var response = httpClient.GetAsync("borrowproducts").Result;
            response.EnsureSuccessStatusCode();
            var products = response.Content.ReadFromJsonAsync<List<BorrowProduct>>().Result;
            return products?.Cast<Product>().ToList() ?? new List<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching borrow products: {ex}");
            return new List<Product>();
        }
    }

    public override Product GetProductById(int id)
    {
        var product = httpClient.GetFromJsonAsync<BorrowProduct>($"borrowproducts/{id}").Result;
        if (product == null)
        {
            throw new KeyNotFoundException($"Borrow product with ID {id} not found.");
        }
        return product;
    }

    public Task<IEnumerable<Product>> SortAndFilter(string sortOption, string filterOption, string filterValue)
    {
        Console.WriteLine("Warning: SortAndFilter not implemented with specific API call yet. Returning all products.");
        return Task.FromResult(GetProducts().Cast<Product>());
    }
}