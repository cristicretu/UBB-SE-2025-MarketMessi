using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using DomainLayer.Domain;
using Microsoft.UI.Xaml.Media.Imaging;
using ViewModelLayer.ViewModel;
using MarketMinds.Views.Pages;

namespace MarketMinds
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BuyProductView : Window
    {
        private readonly BuyProduct buyProduct;
        private readonly User currentUser;
        private readonly BasketViewModel basketViewModel = App.BasketViewModel;

        private Window? seeSellerReviewsView;

        private const int IMAGE_HEIGHT = 250;
        private const int TEXT_BLOCK_MARGIN = 4;
        private const int TEXT_BLOCK_PADDING_LEFT = 8;
        private const int TEXT_BLOCK_PADDING_TOP = 4;
        private const int TEXT_BLOCK_PADDING_RIGHT = 8;
        private const int TEXT_BLOCK_PADDING_BOTTOM = 4;

        public BuyProductView(BuyProduct product)
        {
            this.InitializeComponent();
            buyProduct = product;
            currentUser = MarketMinds.App.CurrentUser;
            LoadProductDetails();
            LoadImages();
        }
        private void LoadProductDetails()
        {
            // Basic Info
            TitleTextBlock.Text = buyProduct.Title;
            CategoryTextBlock.Text = buyProduct.Category.DisplayTitle;
            ConditionTextBlock.Text = buyProduct.Condition.DisplayTitle;
            PriceTextBlock.Text = $"{buyProduct.Price:C}";

            // Seller Info
            SellerTextBlock.Text = buyProduct.Seller.Username;
            DescriptionTextBox.Text = buyProduct.Description;

            TagsItemsControl.ItemsSource = buyProduct.Tags.Select(tag =>
            {
                return new TextBlock
                {
                    Text = tag.DisplayTitle,
                    Margin = new Thickness(TEXT_BLOCK_MARGIN),
                    Padding = new Thickness(TEXT_BLOCK_PADDING_LEFT, TEXT_BLOCK_PADDING_TOP, TEXT_BLOCK_PADDING_RIGHT, TEXT_BLOCK_PADDING_BOTTOM),
                };
            }).ToList();
        }

        private void LoadImages()
        {
            ImageCarousel.Items.Clear();
            foreach (var image in buyProduct.Images)
            {
                var newImage = new Microsoft.UI.Xaml.Controls.Image
                {
                    Source = new BitmapImage(new Uri(image.Url)),
                    Stretch = Stretch.Uniform,
                    Height = IMAGE_HEIGHT,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

                ImageCarousel.Items.Add(newImage);
            }
        }

        private void OnAddToBasketClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                basketViewModel.AddToBasket(buyProduct.Id);

                // Show success notification
                BasketNotificationTip.Title = "Success";
                BasketNotificationTip.Subtitle = "Product added to basket successfully!";
                BasketNotificationTip.IconSource = new SymbolIconSource() { Symbol = Symbol.Accept };
                BasketNotificationTip.IsOpen = true;
            }
            catch (Exception basketAdditionException)
            {
                Debug.WriteLine($"Failed to add product to basket: {basketAdditionException.Message}");

                // Show error notification
                BasketNotificationTip.Title = "Error";
                BasketNotificationTip.Subtitle = $"Failed to add product: {basketAdditionException.Message}";
                BasketNotificationTip.IconSource = new SymbolIconSource() { Symbol = Symbol.Accept };
                BasketNotificationTip.IsOpen = true;
            }
        }

        private void OnSeeReviewsClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            App.SeeSellerReviewsViewModel.Seller = buyProduct.Seller;
            // Create a window to host the SeeSellerReviewsView page
            seeSellerReviewsView = new Window();
            seeSellerReviewsView.Content = new SeeSellerReviewsView(App.SeeSellerReviewsViewModel);
            seeSellerReviewsView.Activate();
        }
    }
}