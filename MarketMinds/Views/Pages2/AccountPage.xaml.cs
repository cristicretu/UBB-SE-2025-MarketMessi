using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Marketplace_SE;
using Marketplace_SE.Data;
using Marketplace_SE.Utilities;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.Foundation.Collections;
using DomainLayer.Domain;
using MarketMinds;
using MarketMinds.ViewModels;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Marketplace_SE
{
    public sealed partial class AccountPage : Page
    {
        public AccountPageViewModel ViewModel { get; private set; }
        private ProgressRing loadingRing;

        public AccountPage()
        {
            this.InitializeComponent();

            // Initialize ViewModel with the service from App
            ViewModel = new AccountPageViewModel(App.AccountPageService);

            // Set DataContext for bindings
            this.DataContext = ViewModel;

            // Initialize loading indicator
            loadingRing = new ProgressRing
            {
                IsActive = false,
                Width = 50,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 20)
            };

            // Add the loading ring to the page
            Grid.SetRow(loadingRing, 2);
            ((Grid)this.Content).Children.Add(loadingRing);

            // Subscribe to property changed events from ViewModel
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            this.Loaded += AccountPage_Loaded;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CurrentUser):
                    UpdateUserInfo();
                    break;
                case nameof(ViewModel.Orders):
                    UpdateOrdersUI();
                    break;
                case nameof(ViewModel.IsLoading):
                    loadingRing.IsActive = ViewModel.IsLoading;
                    break;
                case nameof(ViewModel.ErrorMessage):
                    if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
                    {
                        ShowError(ViewModel.ErrorMessage);
                    }
                    break;
            }
        }

        private async void AccountPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Execute the LoadDataCommand when the page loads
            ViewModel.LoadDataCommand.Execute(null);
        }

        private void UpdateUserInfo()
        {
            if (ViewModel.CurrentUser != null)
            {
                var user = ViewModel.CurrentUser;

                // Debug the user data
                System.Diagnostics.Debug.WriteLine($"Updating UI with user data - Username: {user.Username}, Email: {user.Email}, Balance: {user.Balance}");

                // Update the individual text fields
                UserNameText.Text = user.Username;
                UserEmailText.Text = user.Email;

                // Format balance with proper decimal places and dollar sign
                // Convert to double to ensure proper display with decimal places
                double balance = Convert.ToDouble(user.Balance);
                UserBalanceText.Text = $"${balance:F2}";
                System.Diagnostics.Debug.WriteLine($"Formatted balance text: {UserBalanceText.Text}");

                // Set additional styling for elements if needed
                if (balance < 10.0)
                {
                    UserBalanceText.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    UserBalanceText.Foreground = new SolidColorBrush(Colors.Green);
                }
            }
            else
            {
                // Set default values when user is not available
                UserNameText.Text = "Not logged in";
                UserEmailText.Text = "Not available";
                UserBalanceText.Text = "$0.00";
                UserBalanceText.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void UpdateOrdersUI()
        {
            orderList.Children.Clear();

            if (ViewModel.Orders == null || ViewModel.Orders.Count == 0)
            {
                TextBlock noOrdersText = new TextBlock
                {
                    Text = "No orders found.",
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16
                };
                orderList.Children.Add(noOrdersText);
                return;
            }

            foreach (var order in ViewModel.Orders)
            {
                CreateOrderUI(order);
            }
        }

        private void OnButtonClickNavigateAccountPageMainPage(object sender, RoutedEventArgs e)
        {
            // Use the ViewModel's command instead of creating a new window directly
            ViewModel.NavigateToMainCommand.Execute(null);

            // If you need to keep the direct navigation, you can do this:
            var mainWindow = new Microsoft.UI.Xaml.Window();
            mainWindow.Content = new MainMarketplacePage();
            mainWindow.Activate();
        }

        private void CreateOrderUI(UserOrder order)
        {
            Border orderBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 5, 0, 5),
                Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0))
            };

            StackPanel contentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            bool isBuyOrder = order.BuyerId == ViewModel.CurrentUser?.Id;

            // Order Header - Type and ID
            Grid orderHeader = new Grid
            {
                Margin = new Thickness(0, 0, 0, 10)
            };
            orderHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            orderHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            orderHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            Border typeIndicator = new Border
            {
                Background = new SolidColorBrush(isBuyOrder ? Colors.Green : Colors.Red),
                Width = 10,
                Height = 20,
                CornerRadius = new CornerRadius(2),
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            TextBlock typeLabel = new TextBlock
            {
                Text = isBuyOrder ? "Buy Order" : "Sell Order",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock orderIdText = new TextBlock
            {
                Text = $"Order #{order.Id}",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold
            };

            Grid.SetColumn(typeIndicator, 0);
            Grid.SetColumn(typeLabel, 1);
            Grid.SetColumn(orderIdText, 2);

            orderHeader.Children.Add(typeIndicator);
            orderHeader.Children.Add(typeLabel);
            orderHeader.Children.Add(orderIdText);

            // Order Details
            StackPanel detailsPanel = new StackPanel
            {
                Margin = new Thickness(15, 5, 15, 10)
            };

            TextBlock nameText = new TextBlock
            {
                Text = $"Item: {order.Name}",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock descriptionText = new TextBlock
            {
                Text = $"Description: {order.Description}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock priceText = new TextBlock
            {
                Text = $"Price: ${order.Cost:F2}",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.SemiBold
            };

            TextBlock dateText = new TextBlock
            {
                Text = $"Date: {ViewModel.FormatOrderDateTime(order.Created)}",
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock statusText = new TextBlock
            {
                Text = $"Status: {order.OrderStatus}",
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(Colors.DarkBlue)
            };

            detailsPanel.Children.Add(nameText);
            detailsPanel.Children.Add(descriptionText);
            detailsPanel.Children.Add(priceText);
            detailsPanel.Children.Add(dateText);
            detailsPanel.Children.Add(statusText);

            // Buttons Panel
            StackPanel buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 10,
                Margin = new Thickness(0, 5, 0, 5)
            };

            Button viewButton = new Button
            {
                Content = "View Details",
                Width = 120,
                Margin = new Thickness(0, 5, 0, 5)
            };

            viewButton.Click += (s, e) =>
            {
                ViewModel.SelectedOrder = order;
                ViewModel.ViewOrderCommand.Execute(null);
            };

            buttonsPanel.Children.Add(viewButton);

            if (isBuyOrder)
            {
                Button returnButton = new Button
                {
                    Content = "Return Item",
                    Width = 120,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                returnButton.Click += (s, e) =>
                {
                    ViewModel.SelectedOrder = order;
                    ViewModel.ReturnItemCommand.Execute(null);
                };

                buttonsPanel.Children.Add(returnButton);
            }

            contentPanel.Children.Add(orderHeader);
            contentPanel.Children.Add(new Rectangle { Height = 1, Fill = new SolidColorBrush(Colors.LightGray), Margin = new Thickness(0, 0, 0, 10) });
            contentPanel.Children.Add(detailsPanel);
            contentPanel.Children.Add(buttonsPanel);

            orderBorder.Child = contentPanel;
            orderBorder.Tag = order;

            // Add to list
            orderList.Children.Add(orderBorder);
        }

        private async void ShowError(string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await errorDialog.ShowAsync();
        }
    }
}
