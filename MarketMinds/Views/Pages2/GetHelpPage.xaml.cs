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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
            Frame.Navigate(typeof(ChatBotPage));
        }
        private void OnButtonClickOpenCSConversation(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserFindCallPage));
        }
        private void OnButtonClickNavigateGetHelpPageMainMarketplacePage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMarketplacePage));
        }
    }
}
