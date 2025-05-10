using System.Text.Json.Serialization;
using DataAccessLayer; // Add namespace for DataBaseConnection
using MarketMinds.Repositories.AuctionProductsRepository;
using MarketMinds.Repositories.BuyProductsRepository;
using MarketMinds.Repositories.BasketRepository;
using MarketMinds.Repositories.ReviewRepository;
using MarketMinds.Repositories.ProductCategoryRepository;
using MarketMinds.Repositories.ProductConditionRepository;
using MarketMinds.Repositories.ProductTagRepository;
using MarketMinds.Repositories.ConversationRepository; // Added from luca
using MarketMinds.Repositories.MessageRepository;      // Added from luca
using MarketMinds.Repositories.ChatbotRepository;      // Added new ChatbotRepository
using MarketMinds.Shared.IRepository; // Added for shared interfaces
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;
using Server.MarketMinds.Repositories.BorrowProductsRepository;
using Server.MarketMinds.Repositories.AccountRepository;
using MarketMinds.Shared.Services.DreamTeam.ChatbotService; // Add ChatbotService namespace
using Microsoft.AspNetCore.Identity;
using MarketMinds.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

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

// EntityFramework database connection setup
var initialCatalog = builder.Configuration["InitialCatalog"];
var localDataSource = builder.Configuration["LocalDataSource"];
var connectionString = $"Server={localDataSource};Database={initialCatalog};Trusted_Connection=True;";
builder.Services.AddDbContext<Server.DataAccessLayer.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity services
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = "YourSuperSecretKeyHereThatIsAtLeast32CharsLong";
}

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // Set clockskew to zero so tokens expire exactly at token expiration time
        ClockSkew = TimeSpan.Zero
    };
});

// Register all repositories
builder.Services.AddScoped<IAuctionProductsRepository, AuctionProductsRepository>();
builder.Services.AddScoped<IBuyProductsRepository, BuyProductsRepository>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<MarketMinds.Shared.IRepository.IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<MarketMinds.Shared.IRepository.IProductConditionRepository, ProductConditionRepository>();
builder.Services.AddScoped<IProductTagRepository, ProductTagRepository>();
builder.Services.AddScoped<IBorrowProductsRepository, BorrowProductsRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatbotRepository, ChatbotRepository>();

// Register services
builder.Services.AddScoped<IChatbotService, ChatbotService>();

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

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
