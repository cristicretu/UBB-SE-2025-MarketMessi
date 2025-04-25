using MarketMinds;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViewModelLayer.ViewModel;

namespace Marketplace_SE
{
    public sealed partial class ResetPasswordPage : Page
    {
        public ResetPasswordViewModel ViewModel { get; set; }

        public ResetPasswordPage()
        {
            this.InitializeComponent();
            ViewModel = new ResetPasswordViewModel();
            this.DataContext = ViewModel;
        }

        private void OnResetPasswordClick(object sender, RoutedEventArgs e)
        {
            ViewModel.NewPassword = NewPasswordBox.Password;
            ViewModel.ConfirmPassword = ConfirmPasswordBox.Password;

            if (ViewModel.ResetPassword(ViewModel.NewPassword))
            {
                ViewModel.StatusMessage = "Password reset successfully!";
                Frame.Navigate(typeof(LoginPage));
            }
        }
    }
}