using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marketplace_SE
{
    public sealed partial class EnterIdPage : Page
    {
        public EnterIdPage()
        {
            this.InitializeComponent();
        }

        private string RetrieveUserId()
        {
            // Fake method to retrieve user ID
            return "12345";
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            string enteredId = IdInputBox.Text;

            if (enteredId == RetrieveUserId()) 
            {
                Frame.Navigate(typeof(ResetPasswordPage));
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Invalid ID",
                    Content = "The ID you entered is not valid.",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

    }

}