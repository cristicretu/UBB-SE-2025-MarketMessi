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

using Marketplace_SE.Data;
using Marketplace_SE.Objects;
using Marketplace_SE.Utilities;

using Microsoft.UI;
using Microsoft.UI.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.


namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountPage : Page
    {
        private User me;
        private UserOrder selected_order;

        public AccountPage()
        {
            //hardcoded
            this.me = new User("test", "");
            this.me.SetId(0);

            Database.database = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            bool status = Database.database.Connect();

            if (!status)
            {
                //database connection failed
                //ShowDialog("Database connection error", "Error connecting to database");

                Notification notification = new Notification("Database connection error", "Error connecting to database");
                notification.OkButton.Click += (s, e) =>
                {
                    notification.GetWindow().Close();
                    Database.database.Close();
                    Frame.Navigate(typeof(MainMarketplacePage));
                };
                notification.GetWindow().Activate();
                return;
            }

            var data = Database.database.Get("SELECT * FROM Orders WHERE sellerId=@MyId OR buyerId=@MyId", new string[]
            {
                "@MyId"
            }, new object[]
            {
                this.me.id
            });

            List<UserOrder> orders = Database.database.ConvertToObject<UserOrder>(data);

            //sort after creation time descending
            orders.Sort((a, b) =>
            {
                return (int)(b.created - a.created);
            });


            this.InitializeComponent();

            UserInfoText.Text = "Name - " + this.me.username;

            for (int i = 0; i < orders.Count; i++)
            {
                createOrderUI(orders[i]);
            }
        }

       

        private void createOrderUI(UserOrder order)
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

            bool isBuyOrder = order.buyerId == me.id;

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
                Text = $"{order.name} - {order.description} - ${order.cost:F2} - {DataEncoder.ConvertTimestampToLocalDateTime(order.created)}",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };

            TextBlock statusText = new TextBlock
            {
                Text = $"Status: {order.orderStatus}",
                HorizontalAlignment = HorizontalAlignment.Left,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
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
                selected_order = order;
                OnButtonClickViewOrder(s, e);
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
                    selected_order = order;
                    OnButtonClickReturnItem(s, e);
                };

                contentPanel.Children.Add(returnButton);
            }

            orderBorder.Child = contentPanel;
            orderBorder.Tag = order;

            // final , add to list
            orderList.Children.Add(orderBorder);
        }

        private void OnButtonClickViewOrder(object sender, RoutedEventArgs e)
        {
            //in selected_order is the clicked order
            Frame.Navigate(typeof(PlacedOrderPage));
        }
        private void OnButtonClickReturnItem(object sender, RoutedEventArgs e)
        {
            //in selected_order is the clicked order
            Frame.Navigate(typeof(ReturnItemPage));
        }

        private void OnButtonClickNavigateAccountPageMainPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMarketplacePage));
        }
    }
}
