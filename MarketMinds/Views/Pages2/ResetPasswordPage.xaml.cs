using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Marketplace_SE
{
    public sealed partial class ResetPasswordPage : Page
    {
        public ResetPasswordPage()
        {
            this.InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (newPassword != confirmPassword)
            {
                ShowDialog("Error", "Passwords do not match.");
                return;
            }
            if (newPassword.Length < 8)
            {
                ShowDialog("Error", "Password must be at least 8 characters long.");
                return;
            }
            if (!newPassword.Any(char.IsDigit))
            {
                ShowDialog("Error", "Password must contain at least one digit.");
                return;
            }
            if (!newPassword.Any(char.IsUpper))
            {
                ShowDialog("Error", "Password must contain at least one uppercase letter.");
                return;
            }

            UpdateUserPassword(/*UserID,*/newPassword);
            ShowDialog("Success", "Your password has been reset.");
            Frame.Navigate(typeof(LoginPage));
        }

        private void UpdateUserPassword(string newPassword)
        {
            return;
        }

        private async void ShowDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = NewPasswordBox.Password;
            UpdateRequirements(password);
        }

        private void UpdateRequirements(string password)
        {
            int strength = 0;
            if (password.Length >= 8) strength++;
            if (password.Any(char.IsDigit)) strength++;
            if (password.Any(char.IsUpper)) strength++;

            StrengthBar.Value = strength;

            RequirementsText.Text =
                $"• Min 8 chars: {(password.Length >= 8 ? "✅" : "❌")}  " +
                $"• 1 digit: {(password.Any(char.IsDigit) ? "✅" : "❌")}  " +
                $"• 1 capital: {(password.Any(char.IsUpper) ? "✅" : "❌")}";

        }
    }
}