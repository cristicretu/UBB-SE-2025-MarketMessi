using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using DomainLayer.Domain;
using Marketplace_SE.Utilities; // For Notification, FrameNavigation
using MarketMinds.ViewModels; // For MarketplaceViewModel
using MarketMinds; // For App access (ViewModel, User)

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info
namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
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
            catch (Exception ex)
            {
                ShowNotification("Error", $"An error occurred while loading items: {ex.Message}");
            }
            finally
            {
                LoadingIndicator.IsActive = false;
            }
        }

        private void BuyItemButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (sender as Button)?.DataContext as UserNotSoldOrder;
            if (selectedItem != null)
            {
                Frame.Navigate(typeof(FinalizeOrderPage), selectedItem);
            }
        }

        private void ChatWithSellerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is UserNotSoldOrder selectedOrder)
            {
                int chatTemplateParameter = -1;
                if (selectedOrder.SellerId == 1 && me.Id == 0)
                {
                    chatTemplateParameter = 0;
                }
                else if (selectedOrder.SellerId == 0 && me.Id == 1)
                {
                    chatTemplateParameter = 1;
                }
                else if (selectedOrder.SellerId == 3 && me.Id == 2)
                {
                    chatTemplateParameter = 2;
                }
                else if (selectedOrder.SellerId == 2 && me.Id == 3)
                {
                    chatTemplateParameter = 3;
                }

                if (chatTemplateParameter != -1)
                {
                    Frame.Navigate(typeof(ChatPage), chatTemplateParameter);
                }
                else
                {
                    ShowNotification("Error", "Failed to navigate to chat.");
                }
            }
        }

        private void OnButtonClickOpenAccount(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AccountPage));
        }

        private void OnButtonClickOpenHelp(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GetHelpPage));
        }

        private void ShowNotification(string title, string message)
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
                _ = dialog.ShowAsync();
            }
        }
    }
}