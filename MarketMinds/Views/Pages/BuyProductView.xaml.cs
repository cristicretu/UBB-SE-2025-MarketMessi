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
        private readonly BuyProduct privateProduct;
        private readonly User privateCurrentUser;
        private readonly BasketViewModel priv_basketViewModel = App.BasketViewModel;

        private Window? seeSellerReviewsView;

        private const int ImageHeight = 250; // magic numbers removal
        private const int TextBlockMargin = 4;
        private const int TextBlockPaddingLeft = 8;
        private const int TextBlockPaddingTop = 4;
        private const int TextBlockPaddingRight = 8;
        private const int TextBlockPaddingBottom = 4;

        public BuyProductView(BuyProduct product)
        {
            this.InitializeComponent();
            privateProduct = product;
            privateCurrentUser = MarketMinds.App.CurrentUser;
            LoadProductDetails();
            LoadImages();
        }
        private void LoadProductDetails()
        {
            // Basic Info
            TitleTextBlock.Text = privateProduct.Title;
            CategoryTextBlock.Text = privateProduct.Category.DisplayTitle;
            ConditionTextBlock.Text = privateProduct.Condition.DisplayTitle;
            PriceTextBlock.Text = $"{privateProduct.Price:C}";

            // Seller Info
            SellerTextBlock.Text = privateProduct.Seller.Username;
            DescriptionTextBox.Text = privateProduct.Description;

            TagsItemsControl.ItemsSource = privateProduct.Tags.Select(tag =>
            {
                return new TextBlock
                {
                    Text = tag.DisplayTitle,
                    Margin = new Thickness(TextBlockMargin),
                    Padding = new Thickness(TextBlockPaddingLeft, TextBlockPaddingTop, TextBlockPaddingRight, TextBlockPaddingBottom),
                };
            }).ToList();
        }

        private void LoadImages()
        {
            ImageCarousel.Items.Clear();
            foreach (var image in privateProduct.Images)
            {
                var newImage = new Microsoft.UI.Xaml.Controls.Image
                {
                    Source = new BitmapImage(new Uri(image.Url)),
                    Stretch = Stretch.Uniform, // ✅ shows full image without cropping
                    Height = ImageHeight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

                ImageCarousel.Items.Add(newImage);
            }
        }

        private void OnAddtoBascketClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                priv_basketViewModel.AddToBasket(privateProduct.Id);

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
            App.SeeSellerReviewsViewModel.Seller = privateProduct.Seller;
            // Create a window to host the SeeSellerReviewsView page
            seeSellerReviewsView = new Window();
            seeSellerReviewsView.Content = new SeeSellerReviewsView(App.SeeSellerReviewsViewModel);
            seeSellerReviewsView.Activate();
        }
    }
}