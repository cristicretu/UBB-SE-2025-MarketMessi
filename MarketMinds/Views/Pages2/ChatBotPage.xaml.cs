using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Text;
using DomainLayer.Domain;
using MarketMinds.ViewModels;
using MarketMinds;

namespace Marketplace_SE
{
    public sealed partial class ChatBotPage : Page
    {
        private readonly ChatBotViewModel chatBotViewModel;

        public ChatBotPage()
        {
            this.InitializeComponent();

            chatBotViewModel = App.ChatBotViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
            chatBotViewModel.InitializeChat();
            UpdateChatUI();
        }

        private void ChatBotOptionButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button clickedButton && clickedButton.Tag is Node selectedNode)
            {
                if (chatBotViewModel.SelectOption(selectedNode))
                {
                    UpdateChatUI();
                }
            }
        }

        private void OnButtonClickChatBotKill(object sender, RoutedEventArgs eventArgs)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void UpdateChatUI()
        {
            // Get current state
            string currentResponse = chatBotViewModel.GetCurrentResponse();
            IEnumerable<Node> currentOptions = chatBotViewModel.GetCurrentOptions();
            bool isActive = chatBotViewModel.IsChatInteractionActive();

            // Update response text
            ChatBotChatInterface.Document.SetText(TextSetOptions.None, currentResponse);

            // Update options
            if (isActive && currentOptions != null && currentOptions.Any())
            {
                ChatBotOptionsItemsControl.ItemsSource = currentOptions;
                ChatBotOptionsItemsControl.Visibility = Visibility.Visible;
            }
            else
            {
                ChatBotOptionsItemsControl.ItemsSource = null;
                ChatBotOptionsItemsControl.Visibility = Visibility.Collapsed;
            }
        }
    }
}