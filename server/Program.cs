using DataAccessLayer; // Add namespace for DataBaseConnection
using MarketMinds.Repositories.AuctionProductsRepository; // Add namespace for repository
using MarketMinds.Repositories.BuyProductsRepository; // Add namespace for BuyProductsRepository

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register DataBaseConnection (Singleton is usually appropriate if thread-safe)
// It reads configuration internally now
builder.Services.AddSingleton<DataBaseConnection>(); 

// Register AuctionProductsRepository
builder.Services.AddScoped<IAuctionProductsRepository, AuctionProductsRepository>();
// Register BuyProductsRepository
builder.Services.AddScoped<IBuyProductsRepository, BuyProductsRepository>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors();

// Comment out HTTPS redirection to avoid issues in development
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
