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

        private const int ThicknessOne = 1;  // magic numbers removal
        private const int CornerRadiusTen = 10;
        private const int CornerRadiusTwo = 2;
        private const int PaddingTen = 10;
        private const int FontSizeSixteen = 16;

        private const int TypeIndicatorWidth = 10;
        private const int TypeIndicatorHeight = 20;

        private const int MarginTwenty = 20;
        private const int MarginFifteen = 15;
        private const int MarginTen = 10;
        private const int MarginFive = 5;
        private const int MarginZero = 0;

        private const int ConstantValueZero = 0;
        private const int ConstantValueOne = 1;
        private const int ConstantValueTwo = 2;

        private const int WidthFifty = 50;
        private const int HeightFifty = 50;
        private const int EmptyOrdersCount = 0;

        private const double MinBalance = 10.0;

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
                Width = WidthFifty,
                Height = HeightFifty,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(MarginZero, MarginTwenty, MarginZero, MarginTwenty)
            };

            // Add the loading ring to the page
            Grid.SetRow(loadingRing, ConstantValueTwo);
            ((Grid)this.Content).Children.Add(loadingRing);

            // Subscribe to property changed events from ViewModel
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            this.Loaded += AccountPage_Loaded;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
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

        private async void AccountPage_Loaded(object sender, RoutedEventArgs routedEventArgs)
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
                if (balance < MinBalance)
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

            System.Diagnostics.Debug.WriteLine($"UpdateOrdersUI called - Orders count: {ViewModel.Orders?.Count ?? EmptyOrdersCount}");

            if (ViewModel.Orders == null || ViewModel.Orders.Count == EmptyOrdersCount)
            {
                TextBlock noOrdersText = new TextBlock
                {
                    Text = "No orders found.",
                    Margin = new Thickness(MarginTen),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = FontSizeSixteen
                };
                orderList.Children.Add(noOrdersText);
                System.Diagnostics.Debug.WriteLine("Added 'No orders found' text to UI");
                return;
            }

            foreach (var order in ViewModel.Orders)
            {
                System.Diagnostics.Debug.WriteLine($"Creating UI for order: ID={order.Id}, Name={order.Name}, Cost={order.Cost}");
                CreateOrderUI(order);
            }

            System.Diagnostics.Debug.WriteLine($"Added {ViewModel.Orders.Count} orders to UI");
        }

        private void OnButtonClickNavigateAccountPageMainPage(object sender, RoutedEventArgs routedEventArgs)
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
                BorderThickness = new Thickness(ThicknessOne),
                CornerRadius = new CornerRadius(CornerRadiusTen),
                Padding = new Thickness(PaddingTen),
                Margin = new Thickness(MarginZero, MarginFive, MarginZero, MarginFive),
                Background = new SolidColorBrush(Color.FromArgb(MarginTwenty, MarginZero, MarginZero, MarginZero))
            };

            StackPanel contentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            bool isBuyOrder = order.BuyerId == ViewModel.CurrentUser?.Id;

            // Order Header - Type and ID
            Grid orderHeader = new Grid
            {
                Margin = new Thickness(MarginZero, MarginZero, MarginZero, MarginTen)
            };
            orderHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ConstantValueOne, GridUnitType.Auto) });
            orderHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ConstantValueOne, GridUnitType.Star) });
            orderHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ConstantValueOne, GridUnitType.Auto) });

            Border typeIndicator = new Border
            {
                Background = new SolidColorBrush(isBuyOrder ? Colors.Green : Colors.Red),
                Width = TypeIndicatorWidth,
                Height = TypeIndicatorHeight,
                CornerRadius = new CornerRadius(CornerRadiusTwo),
                Margin = new Thickness(MarginZero, MarginZero, MarginFive, MarginZero),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            TextBlock typeLabel = new TextBlock
            {
                Text = isBuyOrder ? "Buy Order" : "Sell Order",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(MarginFive, MarginZero, MarginZero, MarginZero),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock orderIdText = new TextBlock
            {
                Text = $"Order #{order.Id}",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold
            };

            Grid.SetColumn(typeIndicator, ConstantValueZero);
            Grid.SetColumn(typeLabel, ConstantValueOne);
            Grid.SetColumn(orderIdText, ConstantValueTwo);

            orderHeader.Children.Add(typeIndicator);
            orderHeader.Children.Add(typeLabel);
            orderHeader.Children.Add(orderIdText);

            // Order Details
            StackPanel detailsPanel = new StackPanel
            {
                Margin = new Thickness(MarginFifteen, MarginFive, MarginFifteen, MarginTen)
            };

            TextBlock nameText = new TextBlock
            {
                Text = $"Item: {order.Name}",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(MarginZero, MarginZero, MarginZero, MarginFive)
            };

            TextBlock descriptionText = new TextBlock
            {
                Text = $"Description: {order.Description}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(MarginZero, MarginZero, MarginZero, MarginFive)
            };

            TextBlock priceText = new TextBlock
            {
                Text = $"Price: ${order.Cost:F2}",
                Margin = new Thickness(MarginZero, MarginZero, MarginZero, MarginFive),
                FontWeight = FontWeights.SemiBold
            };

            TextBlock statusText = new TextBlock
            {
                Text = $"Status: {order.OrderStatus}",
                Margin = new Thickness(MarginZero, MarginZero, MarginZero, MarginFive),
                Foreground = new SolidColorBrush(Colors.LightGreen),
            };

            detailsPanel.Children.Add(nameText);
            detailsPanel.Children.Add(descriptionText);
            detailsPanel.Children.Add(priceText);
            detailsPanel.Children.Add(statusText);

            contentPanel.Children.Add(orderHeader);
            contentPanel.Children.Add(new Rectangle { Height = ConstantValueOne, Fill = new SolidColorBrush(Colors.LightGray), Margin = new Thickness(0, 0, 0, 10) });
            contentPanel.Children.Add(detailsPanel);

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
