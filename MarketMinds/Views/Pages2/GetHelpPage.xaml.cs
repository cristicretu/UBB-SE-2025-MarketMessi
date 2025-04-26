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

namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GetHelpPage : Page
    {
        public GetHelpPage()
        {
            this.InitializeComponent();
        }
        private void OnButtonClickOpenChatbotConversation(object sender, RoutedEventArgs e)
        {
            var chatbotWindow = new Window();
            chatbotWindow.Content = new ChatBotPage();
            chatbotWindow.Activate();
        }
        private void OnButtonClickOpenCSConversation(object sender, RoutedEventArgs e)
        {
            var customerSupportWindow = new Window();
            customerSupportWindow.Content = new UserFindCallPage();
            customerSupportWindow.Activate();
        }
        private void OnButtonClickNavigateGetHelpPageMainMarketplacePage(object sender, RoutedEventArgs e)
        {
            var mainMarketplaceWindow = new Window();
            mainMarketplaceWindow.Content = new MainMarketplacePage();
            mainMarketplaceWindow.Activate();
        }
    }
}
