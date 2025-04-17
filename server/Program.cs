using DataAccessLayer; // Add namespace for DataBaseConnection
using MarketMinds.Repositories.AuctionProductsRepository; // Add namespace for repository
using Microsoft.EntityFrameworkCore;
using server.DataAccessLayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register DataBaseConnection (Singleton is usually appropriate if thread-safe)
// It reads configuration internally now
builder.Services.AddSingleton<DataBaseConnection>(); 

var InitialCatalog = builder.Configuration["InitialCatalog"];
var LocalDataSource = builder.Configuration["LocalDataSource"];
var connectionString = $"Server={LocalDataSource};Database={InitialCatalog};Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAuctionProductsRepository, AuctionProductsRepository>();

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
