using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Marketplace_SE;
using Marketplace_SE.Data;
using Marketplace_SE.Utilities;
using Marketplace_SE.ViewModel.DreamTeam;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Marketplace_SE
{
    public sealed partial class AccountPage : Page
    {
        private AccountPageViewModel viewModel;

        private const int OrderBorderThickness = 1;  // magic numbers removal
        private const int OrderCornerRadius = 10;
        private const int OrderPadding = 10;
        private const int OrderMarginTop = 5;
        private const int OrderMarginBottom = 5;
        private const int OrderMarginLeft = 0;
        private const int OrderMarginRight = 0;

        private const int TypeIndicatorWidth = 10;
        private const int TypeIndicatorHeight = 20;
        private const int TypeIndicatorCornerRadius = 2;
        private const int TypeIndicatorMarginLeft = 0;
        private const int TypeIndicatorMarginTop = 0;
        private const int TypeIndicatorMarginRight = 5;
        private const int TypeIndicatorMarginBottom = 0;

        private const int TypeLabelMarginLeft = 15;
        private const int TypeLabelMarginTop = 0;
        private const int TypeLabelMarginRight = 0;
        private const int TypeLabelMarginBottom = 0;

        private const int OrderTypeGridMarginBottom = 10;
        private const int OrderTypeGridMarginTop = 0;
        private const int OrderTypeGridMarginLeft = 0;
        private const int OrderTypeGridMarginRight = 0;

        private const int InfoTextMarginBottom = 10;
        private const int InfoTextMarginTop = 0;
        private const int InfoTextMarginLeft = 0;
        private const int InfoTextMarginRight = 0;

        private const int StatusTextMarginBottom = 10;
        private const int StatusTextMarginTop = 0;
        private const int StatusTextMarginLeft = 0;
        private const int StatusTextMarginRight = 0;

        private const int ButtonWidth = 150;
        private const int ButtonHeight = 50;

        private const int ButtonMarginTop = 5;
        private const int ButtonMarginBottom = 5;
        private const int ButtonMarginLeft = 0;
        private const int ButtonMarginRight = 0;

        public AccountPage()
        {
            this.InitializeComponent();
            viewModel = new AccountPageViewModel();
            this.DataContext = viewModel;

            // Initialize the ViewModel
            viewModel.Initialize();

            // Bind orders to the UI
            foreach (var order in viewModel.Orders)
            {
                CreateOrderUI(order);
            }
        }

        private void OnButtonClickNavigateAccountPageMainPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMarketplacePage));
        }

        private void CreateOrderUI(UserOrder order)
        {
            Border orderBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(OrderBorderThickness),
                CornerRadius = new CornerRadius(OrderCornerRadius),
                Padding = new Thickness(OrderPadding),
                Margin = new Thickness(OrderMarginLeft, OrderMarginTop, OrderMarginRight, OrderMarginBottom)
            };

            StackPanel contentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            bool isBuyOrder = order.BuyerId == viewModel.Me.GetId();

            Grid orderTypeGrid = new Grid
            {
                Margin = new Thickness(OrderTypeGridMarginLeft, OrderTypeGridMarginTop, OrderTypeGridMarginRight, OrderTypeGridMarginBottom)
            };

            Border typeIndicator = new Border
            {
                Background = new SolidColorBrush(isBuyOrder ? Colors.Green : Colors.Red),
                Width = TypeIndicatorWidth,
                Height = TypeIndicatorHeight,
                CornerRadius = new CornerRadius(TypeIndicatorCornerRadius),
                Margin = new Thickness(TypeIndicatorMarginLeft, TypeIndicatorMarginTop, TypeIndicatorMarginRight, TypeIndicatorMarginBottom),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            TextBlock typeLabel = new TextBlock
            {
                Text = isBuyOrder ? "Buy Order" : "Sell Order",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(TypeLabelMarginLeft, TypeLabelMarginTop, TypeLabelMarginRight, TypeLabelMarginBottom),
                VerticalAlignment = VerticalAlignment.Center
            };

            orderTypeGrid.Children.Add(typeIndicator);
            orderTypeGrid.Children.Add(typeLabel);

            TextBlock orderInfo = new TextBlock
            {
                Text = $"{order.Name} - {order.Description} - ${order.Cost:F2} - {DataEncoder.ConvertTimestampToLocalDateTime(order.Created)}",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(InfoTextMarginLeft, InfoTextMarginTop, InfoTextMarginRight, InfoTextMarginBottom)
            };

            TextBlock statusText = new TextBlock
            {
                Text = $"Status: {order.OrderStatus}",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(StatusTextMarginLeft, StatusTextMarginTop, StatusTextMarginRight, StatusTextMarginBottom)
            };

            Button viewButton = new Button
            {
                Content = "View order",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = ButtonWidth,
                Height = ButtonHeight,
                Margin = new Thickness(ButtonMarginLeft, ButtonMarginTop, ButtonMarginRight, ButtonMarginBottom)
            };

            viewButton.Click += (sender, eventArgs) =>
            {
                viewModel.SelectedOrder = order;
                Frame.Navigate(typeof(PlacedOrderPage));
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
                    Margin = new Thickness(ButtonMarginLeft, ButtonMarginTop, ButtonMarginRight, ButtonMarginBottom)
                };

                returnButton.Click += (s, e) =>
                {
                    viewModel.SelectedOrder = order;
                    Frame.Navigate(typeof(ReturnItemPage));
                };

                contentPanel.Children.Add(returnButton);
            }

            orderBorder.Child = contentPanel;
            orderBorder.Tag = order;

            // Add to list
            orderList.Children.Add(orderBorder);
        }
    }
}
