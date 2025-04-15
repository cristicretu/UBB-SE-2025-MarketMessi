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
using Windows.Security.Cryptography.Core;
using Marketplace_SE.Service;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewHelpTicket : Page
    {
        HelpTicket loadedTicket;

        public ViewHelpTicket()
        {
            this.InitializeComponent();

            TextBlockViewHelpTicketDescriptionModificationFailed.Visibility = Visibility.Collapsed;
            TextBlockViewHelpTicketDescriptionModificationSucceeded.Visibility = Visibility.Collapsed;
            TextBlockViewHelpTicketTicketClosureFailed.Visibility = Visibility.Collapsed;
            TextBlockViewHelpTicketTicketClosureSucceeded.Visibility = Visibility.Collapsed;
        }
        private void OnButtonClickNavigateViewHelpTicketPageAdminAccountPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdminAccountPage));
        }
        private void OnButtonClickViewHelpTicketSaveDescriptionModifications(object sender, RoutedEventArgs e)
        {
            int successCode = BackendUserGetHelp.UpdateHelpTicketDescriptionInDB(loadedTicket.TicketID, TextBoxViewHelpTicketDescription.Text);

            if (successCode == (int)BackendUserGetHelp.BackendUserGetHelpStatusCodes.UpdateHelpTicketInDBFailure)
            {
                TextBlockViewHelpTicketDescriptionModificationFailed.Visibility = Visibility.Visible;
            }
            if (successCode == (int)BackendUserGetHelp.BackendUserGetHelpStatusCodes.UpdateHelpTicketInDBSuccess)
            {
                TextBlockViewHelpTicketDescriptionModificationSucceeded.Visibility = Visibility.Visible;
                ButtonViewHelpTicketSaveDescriptionModifications.Visibility = Visibility.Collapsed;
            }
        }
        private void OnTextChangedTextBoxViewHelpTicketDescription(object sender, RoutedEventArgs e)
        {
            ButtonViewHelpTicketSaveDescriptionModifications.Visibility = Visibility.Visible;
        }
        private void OnButtonClickViewHelpTicketCloseTicket(object sender, RoutedEventArgs e)
        {
            int successCode = BackendUserGetHelp.CloseHelpTicketInDB(loadedTicket.TicketID);

            if(successCode == (int)BackendUserGetHelp.BackendUserGetHelpStatusCodes.ClosedHelpTicketInDBFailure)
            {
                TextBlockViewHelpTicketTicketClosureFailed.Visibility = Visibility.Visible;
            }
            if (successCode == (int)BackendUserGetHelp.BackendUserGetHelpStatusCodes.ClosedHelpTicketInDBSuccess)
            {
                TextBlockViewHelpTicketTicketClosureSucceeded.Visibility = Visibility.Visible;
                TextBoxViewHelpTicketDescription.IsReadOnly = true;
                ButtonViewHelpTicketSaveDescriptionModifications.Visibility = Visibility.Collapsed;
                ButtonViewHelpTicketCloseTicket.Visibility = Visibility.Collapsed;
                ButtonViewHelpTicketSaveDescriptionModifications.Visibility = Visibility.Collapsed;
                TextBoxViewHelpTicketDescription.Text = loadedTicket.Descript;
                TextBlockViewHelpTicketClosed.Text = "Closed: " + loadedTicket.Closed;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            List<string> ticketID = new List<string>();
            ticketID.Add(e.Parameter as string);
            HelpTicket currentTicket = BackendUserGetHelp.LoadTicketsFromDB(ticketID)[0];
            loadedTicket = currentTicket;

            TextBlockViewHelpTicketNumber.Text = "Ticket number: " + currentTicket.TicketID;
            TextBlockViewHelpTicketUserID.Text = "User's ID: " + currentTicket.UserID;
            TextBlockViewHelpTicketUserName.Text = "User's name: " + currentTicket.UserName;
            TextBlockViewHelpTicketDateAndTime.Text = "Date and time: " + currentTicket.DateAndTime;
            TextBoxViewHelpTicketDescription.Text = currentTicket.Descript;
            TextBlockViewHelpTicketClosed.Text = "Closed: " + currentTicket.Closed;

            if(loadedTicket.Closed == "No")
            {
                ButtonViewHelpTicketCloseTicket.Visibility = Visibility.Visible;
            }
            else
            {
                TextBoxViewHelpTicketDescription.IsReadOnly = true;
            }
        }
    }
}
