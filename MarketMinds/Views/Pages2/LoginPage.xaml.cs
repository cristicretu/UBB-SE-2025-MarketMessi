using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViewModelLayer.ViewModel;

namespace Marketplace_SE
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; set; }
        public LoginPage()
        {
            this.InitializeComponent();
            ViewModel = new LoginViewModel();
            this.DataContext = ViewModel;
        }

        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            PasswordBoxWithRevealMode.PasswordRevealMode = RevealModeCheckBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
        }

        private void NavigateToSignUpPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SignUpPage));
        }

        private void OnLoginButtonClick(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBoxWithRevealMode.Password;
            Console.WriteLine(username);
            ViewModel.AttemptLogin(username, password);
            if (ViewModel.LoginStatus)
            {
                MarketMinds.App.CurrentUser = ViewModel.LoggedInUser;
                LoginStatusMessage.Text = "You have successfully logged in!";
                LoginStatusMessage.Visibility = Visibility.Visible;
                // Frame.Navigate(typeof());
            }
            else
            {
                LoginStatusMessage.Text = "Invalid username or password.";
                LoginStatusMessage.Visibility = Visibility.Visible;
            }
        }

        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ResetPasswordPage));
        }

        private async Task ShowDialog(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
