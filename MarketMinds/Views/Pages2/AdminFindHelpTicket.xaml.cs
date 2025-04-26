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

        private const int BorderThicknessValue = 2;  // magic numbers removal
        private const int BorderCornerRadius = 5;
        private const int BorderPadding = 10;
        private const int BorderMargin = 5;
        private const int TicketFontSize = 16;
        private const int TextBlockBottomMargin = 10;
        private const int TextBlockLeftMargin = 0;
        private const int TextBlockTopMargin = 0;
        private const int TextBlockRightMargin = 0;
        private const byte BorderBrushAlpha = 255;
        private const byte BorderBrushRed = 130;
        private const byte BorderBrushGreen = 130;
        private const byte BorderBrushBlue = 130;

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
                BorderThickness = new Thickness(BorderThicknessValue),
                CornerRadius = new CornerRadius(BorderCornerRadius),
                BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(BorderBrushAlpha, BorderBrushRed, BorderBrushGreen, BorderBrushBlue)),
                Padding = new Thickness(BorderPadding),
                Margin = new Thickness(BorderMargin),
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
                Margin = new Thickness(TextBlockLeftMargin, TextBlockTopMargin, TextBlockRightMargin, TextBlockBottomMargin),
                FontSize = TicketFontSize,
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
