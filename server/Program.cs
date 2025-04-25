using DataAccessLayer; // Add namespace for DataBaseConnection
using MarketMinds.Repositories.AuctionProductsRepository;
using MarketMinds.Repositories.ReviewRepository;
using MarketMinds.Repositories.BuyProductsRepository;
using MarketMinds.Repositories.BasketRepository; // Add namespace for BasketRepository
using Microsoft.EntityFrameworkCore;
using server.DataAccessLayer;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;

    // Enable camel casing to match frontend expectations
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

    // Ignore null values in the output
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// EntityFramework database connection setup
var InitialCatalog = builder.Configuration["InitialCatalog"];
var LocalDataSource = builder.Configuration["LocalDataSource"];
var connectionString = $"Server={LocalDataSource};Database={InitialCatalog};Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddScoped<IAuctionProductsRepository, AuctionProductsRepository>();
builder.Services.AddScoped<IBuyProductsRepository, BuyProductsRepository>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
