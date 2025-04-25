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
    public sealed partial class AdminFindHelpTicket : Page
    {
        private AdminFindHelpTicketViewModel viewModel;

        public AdminFindHelpTicket()
        {
            this.InitializeComponent();
            viewModel = new AdminFindHelpTicketViewModel();
            this.DataContext = viewModel;
        }

        private void OnButtonClickAdminSearchHelpTicket(object sender, RoutedEventArgs e)
        {
            viewModel.SearchHelpTickets(TextBoxLookupHelpTicketUserID.Text);

            // Update UI based on ViewModel state
            TextBlockAdminFindHelpTicketUserIDNotFound.Visibility = viewModel.IsUserIdNotFound ? Visibility.Visible : Visibility.Collapsed;
            TextBlockAdminFindHelpTicketTypeUserID.Visibility = viewModel.IsUserIdInvalid ? Visibility.Visible : Visibility.Collapsed;

            if (!viewModel.HasErrors)
            {
                StackPanelAdminFindHelpTickets.Children.Clear();

                foreach (var ticket in viewModel.HelpTickets)
                {
                    CreateHelpTicketUI(ticket);
                }
            }
        }

        private void CreateHelpTicketUI(HelpTicket ticket)
        {
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
                Text = ticket.ToStringExceptDescription(),
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Button button = new Button
            {
                Content = "View ticket",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            button.Click += (s, e) =>
            {
                Frame.Navigate(typeof(ViewHelpTicket), ticket.TicketID);
            };

            innerStackPanel.Children.Add(textBlock);
            innerStackPanel.Children.Add(button);

            border.Child = innerStackPanel;

            StackPanelAdminFindHelpTickets.Children.Add(border);
        }

        private void OnButtonClickNavigateAdminSearchHelpTicketPageAdminAccountPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdminAccountPage));
        }
    }
}
