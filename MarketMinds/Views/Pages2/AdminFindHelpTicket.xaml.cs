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

        private const int BORDER_THICKNESS = 2;
        private const int BORDER_CORNER_RADIUS = 5;
        private const int BORDER_PADDING = 10;
        private const int BORDER_MARGIN = 5;
        private const int TEXT_BLOCK_FONT_SIZE = 16;
        private const int TEXT_BLOCK_MARGIN_BOTTOM = 10;
        private const int TEXT_BLOCK_MARGIN_LEFT = 0;
        private const int TEXT_BLOCK_MARGIN_TOP = 0;
        private const int TEXT_BLOCK_MARGIN_RIGHT = 0;
        private const byte BORDER_BRUSH_ALPHA = 255;
        private const byte BORDER_BRUSH_RED = 130;
        private const byte BORDER_BRUSH_GREEN = 130;
        private const byte BORDER_BRUSH_BLUE = 130;

        public AdminFindHelpTicket()
        {
            this.InitializeComponent();
            viewModel = new AdminFindHelpTicketViewModel();
            this.DataContext = viewModel;
        }

        private void OnButtonClickAdminSearchHelpTicket(object sender, RoutedEventArgs routedEventArgs)
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
                BorderThickness = new Thickness(BORDER_THICKNESS),
                CornerRadius = new CornerRadius(BORDER_CORNER_RADIUS),
                BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(BORDER_BRUSH_ALPHA, BORDER_BRUSH_RED, BORDER_BRUSH_GREEN, BORDER_BRUSH_BLUE)),
                Padding = new Thickness(BORDER_PADDING),
                Margin = new Thickness(BORDER_MARGIN),
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
                Margin = new Thickness(TEXT_BLOCK_MARGIN_LEFT, TEXT_BLOCK_MARGIN_TOP, TEXT_BLOCK_MARGIN_RIGHT, TEXT_BLOCK_MARGIN_BOTTOM),
                FontSize = TEXT_BLOCK_FONT_SIZE,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Button button = new Button
            {
                Content = "View ticket",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            button.Click += (sender, eventArgs) =>
            {
                Frame.Navigate(typeof(ViewHelpTicket), ticket.TicketID);
            };

            innerStackPanel.Children.Add(textBlock);
            innerStackPanel.Children.Add(button);

            border.Child = innerStackPanel;

            StackPanelAdminFindHelpTickets.Children.Add(border);
        }

        private void OnButtonClickNavigateAdminSearchHelpTicketPageAdminAccountPage(object sender, RoutedEventArgs routedEventArgs)
        {
            Frame.Navigate(typeof(AdminAccountPage));
        }
    }
}
