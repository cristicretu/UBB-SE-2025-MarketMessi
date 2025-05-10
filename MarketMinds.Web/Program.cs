using Microsoft.AspNetCore.Identity;
using MarketMinds.Shared;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.ProxyRepository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register shared service clients
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/api/");
});

// Register AuctionProductsProxyRepository
builder.Services.AddSingleton<AuctionProductsProxyRepository>();
// Register BorrowProductsProxyRepository
builder.Services.AddSingleton<BorrowProductsProxyRepository>();

// Register AuctionProductService
builder.Services.AddTransient<IAuctionProductService, MarketMinds.Shared.Services.AuctionProductsService.AuctionProductsService>();
// Register BorrowProductService
builder.Services.AddTransient<IBorrowProductsService, MarketMinds.Shared.Services.BorrowProductsService.BorrowProductsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();