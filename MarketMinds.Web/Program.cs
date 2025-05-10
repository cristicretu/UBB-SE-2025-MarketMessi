using Microsoft.AspNetCore.Identity;
using MarketMinds.Shared;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Shared.Services.ProductCategoryService;
using MarketMinds.Shared.Services.ProductConditionService;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.IRepository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register shared service clients
builder.Services.AddHttpClient("ApiClient", client =>
{
    // Get base URL from configuration
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    
    if (string.IsNullOrEmpty(baseUrl))
    {
        baseUrl = "http://localhost:5001";
    }
    
    // Make sure URL ends with slash
    if (!baseUrl.EndsWith("/"))
    {
        baseUrl += "/";
    }
    
    client.BaseAddress = new Uri(baseUrl + "api/");
});

// Register repositories
builder.Services.AddSingleton<AuctionProductsProxyRepository>();
builder.Services.AddSingleton<ProductCategoryProxyRepository>();
builder.Services.AddSingleton<ProductConditionProxyRepository>();

// Register services
builder.Services.AddTransient<IAuctionProductService, AuctionProductsService>();
builder.Services.AddTransient<IProductCategoryService, ProductCategoryService>();
builder.Services.AddTransient<IProductConditionService, ProductConditionService>();

// Register repository interfaces
builder.Services.AddTransient<IProductCategoryRepository>(sp => sp.GetRequiredService<ProductCategoryProxyRepository>());
builder.Services.AddTransient<IProductConditionRepository>(sp => sp.GetRequiredService<ProductConditionProxyRepository>());

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