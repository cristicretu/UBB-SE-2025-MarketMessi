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
using Marketplace_SE.Service;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OpenHelpTicketPage : Page
    {
        public OpenHelpTicketPage()
        {
            this.InitializeComponent();
        }
        private void OnButtonClickAdminOpenHelpTicket(object sender, RoutedEventArgs e)
        {
            TextBlockOpenTicketEmptyFields.Visibility = Visibility.Collapsed;
            TextBlockOpenTicketUserNotFound.Visibility = Visibility.Collapsed;
            TextBlockOpenTicketTicketAddedSuccessfully.Visibility = Visibility.Collapsed;
            TextBlockOpenTicketTicketAddFailed.Visibility = Visibility.Collapsed;

            bool isDataCorrect = true;

            if (TextBoxOpenTicketUserID.Text == "" || TextBoxOpenTicketUserName.Text == "" || TextBoxOpenTicketDescription.Text == "")
            {
                TextBlockOpenTicketEmptyFields.Visibility = Visibility.Visible;
                isDataCorrect = false;
            }
            if(!BackendUserGetHelp.DoesUserIDExist(TextBoxOpenTicketUserID.Text))
            {
                TextBlockOpenTicketUserNotFound.Visibility = Visibility.Visible;
                isDataCorrect = false;
            }

            if(isDataCorrect)
            {
                int returnCode = BackendUserGetHelp.PushNewHelpTicketToDB(TextBoxOpenTicketUserID.Text, TextBoxOpenTicketUserName.Text, TextBoxOpenTicketDescription.Text, "No");
                switch(returnCode)
                {
                    case (int)BackendUserGetHelp.BackendUserGetHelpStatusCodes.PushNewHelpTicketToDBSuccess:
                        {
                            TextBlockOpenTicketTicketAddedSuccessfully.Visibility = Visibility.Visible;
                            break;
                        }
                    case (int)BackendUserGetHelp.BackendUserGetHelpStatusCodes.PushNewHelpTicketToDBFailure:
                        {
                            TextBlockOpenTicketTicketAddFailed.Visibility = Visibility.Visible;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }
        private void OnButtonClickAdminNavigateOpenHelpTicketPageAdminAccountPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdminAccountPage));
        }
    }
}
