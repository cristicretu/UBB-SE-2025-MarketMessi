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
using MarketMinds.ViewModels.DreamTeam;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Marketplace_SE
{
    public sealed partial class AccountPage : Page
    {
        private AccountPageViewModel ViewModel { get; set; }

        public AccountPage()
        {
            this.InitializeComponent();
            ViewModel = new AccountPageViewModel();
            this.DataContext = ViewModel;
            this.Loaded += AccountPage_Loaded;
        }

        private async void AccountPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync();
            PopulateOrdersListFromViewModel();
        }

        private void PopulateOrdersListFromViewModel()
        {
            orderList.Children.Clear();
            if (ViewModel.Orders == null || !ViewModel.Orders.Any())
            {
                orderList.Children.Add(new TextBlock { Text = "No orders found.", Margin = new Thickness(10) });
                return;
            }

            foreach (var order in ViewModel.Orders)
            {
                CreateOrderUI(order);
            }
        }

        private void OnButtonClickNavigateAccountPageMainPage(object sender, RoutedEventArgs e)
        {
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
                Margin = new Thickness(0, 5, 0, 5)
            };

            StackPanel contentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            bool isBuyOrder = order.BuyerId == ViewModel.CurrentUser?.Id;

            Grid orderTypeGrid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 10)
            };

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
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            orderTypeGrid.Children.Add(typeIndicator);
            orderTypeGrid.Children.Add(typeLabel);

            TextBlock orderInfo = new TextBlock
            {
                Text = $"{order.ItemName} - {order.Description} - ${order.Price:F2} - {(DataEncoder.ConvertTimestampToLocalDateTime(order.Created))}",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };

            TextBlock statusText = new TextBlock
            {
                Text = $"Status: {order.OrderStatus}",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            };

            Button viewButton = new Button
            {
                Content = "View order",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 150,
                Height = 50,
                Margin = new Thickness(0, 5, 0, 5)
            };

            viewButton.Click += (s, e) =>
            {
                var orderWindow = new Microsoft.UI.Xaml.Window();
                orderWindow.Content = new PlacedOrderPage(order);
                orderWindow.Activate();
            };

            contentPanel.Children.Add(orderTypeGrid);
            contentPanel.Children.Add(orderInfo);
            contentPanel.Children.Add(statusText);
            contentPanel.Children.Add(viewButton);

            if (isBuyOrder)
            {
                Button returnButton = new Button
                {
                    Content = "Return item",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 150,
                    Height = 50,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                returnButton.Click += (s, e) =>
                {
                    var returnWindow = new Microsoft.UI.Xaml.Window();
                    returnWindow.Content = new ReturnItemPage(order);
                    returnWindow.Activate();
                };

                contentPanel.Children.Add(returnButton);
            }

            orderBorder.Child = contentPanel;
            orderBorder.Tag = order;

            orderList.Children.Add(orderBorder);
        }
    }
}
