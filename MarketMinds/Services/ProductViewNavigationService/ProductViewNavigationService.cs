using System;
using MarketMinds.Shared.Models;
using MarketMinds.Views.Pages;
using Microsoft.UI.Xaml;

namespace MarketMinds.Services
{
    public class ProductViewNavigationService : IProductViewNavigationService
    {
        public Window CreateProductDetailView(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            Window detailView;

            var auctionProduct = product as AuctionProduct;
            var borrowProduct = product as BorrowProduct;
            var buyProduct = product as BuyProduct;

            if (auctionProduct != null)
            {
                detailView = new AuctionProductView(auctionProduct);
            }
            else if (borrowProduct != null)
            {
                detailView = new BorrowProductView(borrowProduct);
            }
            else if (buyProduct != null)
            {
                detailView = new BuyProductView(buyProduct);
            }
            else
            {
                throw new ArgumentException($"Unknown product type: {product.GetType().Name}");
            }

            return detailView;
        }

        public Window CreateSellerReviewsView(User seller)
        {
            if (seller == null)
            {
                throw new ArgumentNullException(nameof(seller));
            }

            var window = new Window();
            window.Content = new SeeSellerReviewsView(App.SeeSellerReviewsViewModel);
            App.SeeSellerReviewsViewModel.Seller = seller;
            return window;
        }
    }
}