using DataAccessLayer; // Add namespace for DataBaseConnection
using MarketMinds.Repositories.AuctionProductsRepository;
using MarketMinds.Repositories.BuyProductsRepository;
using MarketMinds.Repositories.BasketRepository;
using MarketMinds.Repositories.ReviewRepository;
using MarketMinds.Repositories.ProductCategoryRepository;
using MarketMinds.Repositories.ProductConditionRepository;
using MarketMinds.Repositories.ProductTagRepository;
using Microsoft.EntityFrameworkCore;
using server.DataAccessLayer;
using server.MarketMinds.Repositories.BorrowProductsRepository;
using server.MarketMinds.Repositories.AccountRepository; // Added from main
using System.Text.Json.Serialization;
using MarketMinds.Repositories.ConversationRepository;
using MarketMinds.Repositories.MessageRepository;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Do not use ReferenceHandler.Preserve as requested by the user
    // We'll use JsonIgnore attributes on navigation properties instead
    options.JsonSerializerOptions.ReferenceHandler = null;

    // Enable camel casing to match frontend expectations
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

    // Ignore null values in the output
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// compatibility with old API without EF, need to remove this when EF is fully implemented
builder.Services.AddSingleton<DataBaseConnection>();

builder.Services.AddHttpClient(); // <-- Was missing in main, keeping from luca

// EntityFramework database connection setup
var InitialCatalog = builder.Configuration["InitialCatalog"];
var LocalDataSource = builder.Configuration["LocalDataSource"];
var connectionString = $"Server={LocalDataSource};Database={InitialCatalog};Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register all repositories
builder.Services.AddScoped<IAuctionProductsRepository, AuctionProductsRepository>();
builder.Services.AddScoped<IBuyProductsRepository, BuyProductsRepository>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductConditionRepository, ProductConditionRepository>();
builder.Services.AddScoped<IProductTagRepository, ProductTagRepository>();
builder.Services.AddScoped<IBorrowProductsRepository, BorrowProductsRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>(); // Added from main
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

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
