using Microsoft.AspNetCore.Identity;
using MarketMinds.Shared;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.AuctionProductsService;

var builder = WebApplication.CreateBuilder(args);

// Remove database connection and replace with services

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register shared service clients
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/api/");
});

// Register AuctionProductService
builder.Services.AddHttpClient<IAuctionProductService, MarketMinds.Shared.Services.AuctionProductsService.AuctionProductsService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/api/");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
app.MapRazorPages();

app.Run();