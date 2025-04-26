using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using DomainLayer.Domain;
using Marketplace_SE.Utilities; // For Notification, FrameNavigation
using MarketMinds.ViewModels; // For MarketplaceViewModel
using MarketMinds;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info
namespace Marketplace_SE
{
    public sealed partial class MainMarketplacePage : Page
    {
        private readonly MainMarketplaceViewModel mainMarketplaceViewModel;
        private User me;

        public MainMarketplacePage()
        {
            this.InitializeComponent();

            mainMarketplaceViewModel = App.MainMarketplaceViewModel;
            me = App.CurrentUser;
        }

        protected override void OnNavigatedTo(NavigationEventArgs routedEventArgs)
        {
            base.OnNavigatedTo(e);
            LoadAvailableItems();
        }

        private void LoadAvailableItems()
        {
            LoadingIndicator.IsActive = true;
            ItemsListView.ItemsSource = null;

            try
            {
                if (mainMarketplaceViewModel != null)
                {
                    var items = mainMarketplaceViewModel.GetAvailableItems();
                    ItemsListView.ItemsSource = items;
                }
                else
                {
                    ShowNotification("Error", "Failed to load items.");
                }
            }
            catch (Exception itemsLoadingException)
            {
                ShowNotification("Error", $"An error occurred while loading items: {itemsLoadingException.Message}");
            }
            finally
            {
                LoadingIndicator.IsActive = false;
            }
        }

        private void BuyItemButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var selectedItem = (sender as Button)?.DataContext as UserNotSoldOrder;
            if (selectedItem != null)
            {
                Frame.Navigate(typeof(FinalizeOrderPage), selectedItem);
            }
        }

        private void ChatWithSellerButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender is FrameworkElement element && element.Tag is UserNotSoldOrder selectedOrder)
            {
                Frame.Navigate(typeof(ChatPage), selectedOrder);
            }
            else
            {
                ShowNotification("Error", "Failed to navigate to chat.");
            }
        }

        private void OpenAccountButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var accountWindow = new Window();
            accountWindow.Content = new AccountPage();
            accountWindow.Activate();
        }

        private void OpenHelpButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            Frame.Navigate(typeof(GetHelpPage));
        }

        private async Task ShowNotification(string title, string message)
        {
            Notification notification = new Notification(title, message);
            var window = notification.GetWindow();

            if (window != null)
            {
                window.Activate();
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }
}