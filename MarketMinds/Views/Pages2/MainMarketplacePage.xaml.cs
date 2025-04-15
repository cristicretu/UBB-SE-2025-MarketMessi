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
using Marketplace_SE.Objects;
using Marketplace_SE.Utilities;
using Marketplace_SE.Data;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMarketplacePage : Page
    {
        private User me;
        private UserNotSoldOrder selected_item;

        public MainMarketplacePage()
        {
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

            var data = Database.database.Get("SELECT * FROM Orders WHERE buyerId=-1", new string[]
            {
                "@MyId"
            }, new object[]
            {
                this.me.id
            });

            List<UserNotSoldOrder> orders = Database.database.ConvertToObject<UserNotSoldOrder>(data);

            //don't need to sort
            /*
            orders.Sort((a, b) =>
            {
                return (int)(b.created - a.created);
            });
            */


            this.InitializeComponent();

            for(int i = 0; i < orders.Count; i++)
            {
                createUIOrder(orders[i]);
            }
        }




        public void createUIOrder(UserNotSoldOrder order)
        {
            
             Border itemBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(255,118,185,237)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(15),
                Background = new SolidColorBrush(Color.FromArgb(255,0,0,0))
            };

            

            StackPanel contentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8
            };
            TextBlock itemInfo = new TextBlock
            {
                Text = $"{order.name} - {order.description} - ${order.cost:F2}",
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Left
            };


            // DB get seller name
            // but now hardcoded to ID

            TextBlock sellerInfo = new TextBlock
            {
                Text = $"Seller Id: {order.sellerId}",
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.Normal,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                Margin = new Thickness(0, 0, 0, 10)
            };

            Button buyButton = new Button
            {
                Content = "Buy item",
                Background = new SolidColorBrush(Color.FromArgb(255,0,90,158)),
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(15, 8,15,8),
                Margin = new Thickness(0, 5, 0, 5)
            };
            buyButton.Name = $"ButtonBuyItem_{order.id}";
            buyButton.Click += (s, e) =>
            {
                selected_item = order;
                OnButtonClickBuyItem(s, e);
            };
            Button chatButton = new Button
            {
                Content = "Chat with seller",
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(15, 8,15,8)
            };
            chatButton.Name = $"ButtonChatWithSeller_{order.id}";
            chatButton.Click += (s, e) =>
            {
                selected_item = order;
                OnButtonClickChatWithSeller(s, e);
            };

            contentPanel.Children.Add(itemInfo);
            contentPanel.Children.Add(sellerInfo);
            contentPanel.Children.Add(buyButton);
            contentPanel.Children.Add(chatButton);
            itemBorder.Child = contentPanel;
            itemBorder.Tag = order;

            ItemsPanel.Children.Add(itemBorder);
        }



        public void OnButtonClickBuyItem(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FinalizeOrderPage));
        }
        public void OnButtonClickOpenAccount(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AccountPage));
        }
        public void OnButtonClickChatWithSeller(object sender, RoutedEventArgs e)
        {
            //IONUT AND CALIN HERE
            if (selected_item.sellerId == 1)
            {
                FrameNavigation.NavigateWithConstructorParameters<ChatPage>(this.Frame,0);
            } else if(selected_item.sellerId == 0){
                FrameNavigation.NavigateWithConstructorParameters<ChatPage>(this.Frame, 1);
            }
            //Frame.Navigate(typeof(ChatPage));
        }

        public void OnButtonClickOpenHelp(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GetHelpPage));
        }
    }
}
