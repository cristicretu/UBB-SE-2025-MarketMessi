using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Marketplace_SE.ViewModels;
using Marketplace_SE.Services;
using MarketMinds.Repositories.HelpTicketRepository;
using DataAccessLayer;
using MarketMinds;

namespace Marketplace_SE.Views.Pages2
{
    public sealed partial class OpenHelpTicketPage : Page
    {
        public HelpTicketViewModel ViewModel { get; }

        public OpenHelpTicketPage()
        {
            this.InitializeComponent();

            // Initialize the service and repository
            DataBaseConnection connection = App.DatabaseConnection;
            var repository = new HelpTicketRepository(connection);
            var service = new HelpTicketService(repository);

            // Initialize the ViewModel
            ViewModel = new HelpTicketViewModel(service);
            this.DataContext = ViewModel;
        }

        private void OnButtonClickAdminOpenHelpTicket(object sender, RoutedEventArgs routedEventArgs)
        {
            // Reset visibility of status messages
            TextBlockOpenTicketEmptyFields.Visibility = Visibility.Collapsed;
            TextBlockOpenTicketUserNotFound.Visibility = Visibility.Collapsed;
            TextBlockOpenTicketTicketAddedSuccessfully.Visibility = Visibility.Collapsed;
            TextBlockOpenTicketTicketAddFailed.Visibility = Visibility.Collapsed;

            // Validate input fields
            bool isDataCorrect = true;

            if (string.IsNullOrWhiteSpace(TextBoxOpenTicketUserID.Text) ||
                string.IsNullOrWhiteSpace(TextBoxOpenTicketUserName.Text) ||
                string.IsNullOrWhiteSpace(TextBoxOpenTicketDescription.Text))
            {
                TextBlockOpenTicketEmptyFields.Visibility = Visibility.Visible;
                isDataCorrect = false;
            }

            // Validate user ID using the ViewModel
            if (isDataCorrect && !ViewModel.ValidateUser(TextBoxOpenTicketUserID.Text))
            {
                TextBlockOpenTicketUserNotFound.Visibility = Visibility.Visible;
                isDataCorrect = false;
            }

            // Create the ticket using the ViewModel
            if (isDataCorrect)
            {
                ViewModel.UserID = TextBoxOpenTicketUserID.Text;
                ViewModel.UserName = TextBoxOpenTicketUserName.Text;
                ViewModel.Description = TextBoxOpenTicketDescription.Text;

                ViewModel.CreateTicket();

                if (ViewModel.StatusMessage == "Ticket created successfully.")
                {
                    TextBlockOpenTicketTicketAddedSuccessfully.Visibility = Visibility.Visible;
                }
                else if (ViewModel.StatusMessage == "Failed to create ticket.")
                {
                    TextBlockOpenTicketTicketAddFailed.Visibility = Visibility.Visible;
                }
            }
        }

        private void OnButtonClickAdminNavigateOpenHelpTicketPageAdminAccountPage(object sender, RoutedEventArgs routedEventArgs)
        {
            Frame.Navigate(typeof(AdminAccountPage));
        }
    }
}
