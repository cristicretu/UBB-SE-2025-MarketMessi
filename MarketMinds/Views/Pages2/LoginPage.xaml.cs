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
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            TextBlockWrong.Visibility = Visibility.Collapsed;
        }

        private void OnButtonClickLogin(object sender, RoutedEventArgs e)
        {
            if (LineEditUsername.Text == "" && LineEditPassword.Text == "") {
                Frame.Navigate(typeof(MainMarketplacePage));
            }
            else if (LineEditUsername.Text == "admin" && LineEditPassword.Text == "password")
            {
                Frame.Navigate(typeof(AdminAccountPage));
            }
            else if (LineEditUsername.Text == "username" && LineEditPassword.Text == "password")
            {
                Frame.Navigate(typeof(MainMarketplacePage));
            }
            else
            {
                TextBlockWrong.Visibility = Visibility.Visible;
            }
        }
        private void OnButtonClickRegister(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegisterPage));
        }
        private void OnButtonClickResetPassword(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EnterIdPage));
        }
    }
}
