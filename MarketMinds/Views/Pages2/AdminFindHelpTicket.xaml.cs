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
using Microsoft.UI;
using Marketplace_SE.Data;
using Marketplace_SE.Service;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminFindHelpTicket : Page
    {
        public AdminFindHelpTicket()
        {
            this.InitializeComponent();
        }
        private void OnButtonClickAdminSearchHelpTicket(object sender, RoutedEventArgs e)
        {

            TextBlockAdminFindHelpTicketUserIDNotFound.Visibility = Visibility.Collapsed;
            TextBlockAdminFindHelpTicketTypeUserID.Visibility = Visibility.Collapsed;

            bool errorInInput = false;

            if(TextBoxLookupHelpTicketUserID.Text == "")
            {
                TextBlockAdminFindHelpTicketTypeUserID.Visibility = Visibility.Visible;
                errorInInput = true;
            }
            else
                if (!BackendUserGetHelp.DoesUserIDExist(TextBoxLookupHelpTicketUserID.Text))
                {
                    TextBlockAdminFindHelpTicketUserIDNotFound.Visibility = Visibility.Visible;
                    errorInInput = true;
                }

            if(!errorInInput)
            {
                List<string> helpTicketIDs = BackendUserGetHelp.GetTicketIDsMatchingCriteria(TextBoxLookupHelpTicketUserID.Text);
                List<HelpTicket> helpTickets = BackendUserGetHelp.LoadTicketsFromDB(helpTicketIDs);

                StackPanelAdminFindHelpTickets.Children.Clear();

                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(5),
                    BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 130, 130, 130)),
                    Padding = new Thickness(10),
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                StackPanel innerStackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                TextBlock textBlock = new TextBlock
                {
                    Text = "TICKET ID - USER ID - USER'S NAME - DATE AND TIME - CLOSED",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                innerStackPanel.Children.Add(textBlock);
                border.Child = innerStackPanel;

                StackPanelAdminFindHelpTickets.Children.Add(border);

                foreach (HelpTicket each in helpTickets)
                {
                    //construct an item for the scrollable list
                    Border border_ = new Border
                    {
                        BorderThickness = new Thickness(2),
                        CornerRadius = new CornerRadius(5),
                        BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 130, 130, 130)),
                        Padding = new Thickness(10),
                        Margin = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    StackPanel innerStackPanel_ = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    TextBlock textBlock_ = new TextBlock
                    {
                        Text = each.toStringExceptDescription(),
                        Margin = new Thickness(0, 0, 0, 10),
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };

                    Button button_ = new Button
                    {
                        Content = "View ticket",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };

                    button_.Click += OnButtonClickNavigateAdminSearchHelpTicketPageViewTicketPage;

                    innerStackPanel_.Children.Add(textBlock_);
                    innerStackPanel_.Children.Add(button_);

                    border_.Child = innerStackPanel_;

                    StackPanelAdminFindHelpTickets.Children.Add(border_);
                }
            }
        }
        private void OnButtonClickNavigateAdminSearchHelpTicketPageAdminAccountPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdminAccountPage));
        }

        private void OnButtonClickNavigateAdminSearchHelpTicketPageViewTicketPage(object sender, RoutedEventArgs e)
        {
            Button buttonClicked = sender as Button;
            foreach(var each in (buttonClicked.Parent as StackPanel).Children)
            {
                if(each.GetType() == typeof(TextBlock))
                {
                    string text = (each as TextBlock).Text;
                    string[] tokenizedText = text.Split("::");

                    //pass the string TicketID to the new Page to look for it
                    Frame.Navigate(typeof(ViewHelpTicket), tokenizedText[0]);
                }
            }
        }
    }
}
