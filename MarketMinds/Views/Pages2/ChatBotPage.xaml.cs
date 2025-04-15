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
//using ChatBot;
using Microsoft.UI.Text;
using Marketplace_SE.Service;

namespace Marketplace_SE
{
    public sealed partial class ChatBotPage : Page
    {
        private Node currentNode;

        public ChatBotPage()
        {
            this.InitializeComponent();

            ChatBotChatInterface.GotFocus += (s, e) => { this.Focus(FocusState.Programmatic); };    // Deflect focus

            currentNode = ChatBotDataManager.LoadTree();    // Load data from tree (root)
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ChatBotOptionsPanel.Children.Clear();

            ChatBotChatInterface.IsReadOnly = false;    // Allow writing to chat interface
            ChatBotChatInterface.Document.SetText(TextSetOptions.None, currentNode.Response);
            ChatBotChatInterface.IsReadOnly = true;     // Disable writing to chat interface

            CreateChatBotOptions(currentNode.Children);
        }

        private void CreateChatBotOptions(List<Node> children)
        {
            ChatBotOptionsPanel.Children.Clear();

            foreach (Node child in children)
            {
                Grid optionRow = CreateOptionGrid(child);
                ChatBotOptionsPanel.Children.Add(optionRow);
            }
        }

        private Grid CreateOptionGrid(Node child)
        {
            Grid grid = new Grid
            {
                Margin = new Thickness(3),
                MinHeight = 40,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Define column widths
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70, GridUnitType.Star) });

            Button button = new Button
            {
                Content = child.ButtonLabel,
                Tag = child,
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            button.Click += ChatBotButtonClickHandler; // Add on_click handler

            TextBlock textBlock = new TextBlock
            {
                Text = child.LabelText,
                FontSize = 16,
                Padding = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
            };

            Grid.SetColumn(button, 0);
            Grid.SetColumn(textBlock, 1);

            grid.Children.Add(button);
            grid.Children.Add(textBlock);

            return grid;
        }

        private void ChatBotButtonClickHandler(object sender, RoutedEventArgs e)
        {
            // If pressed button is an option
            if (sender is Button clickedButton && clickedButton.Tag is Node node)
            {
                ChatBotChatInterface.IsReadOnly = false;
                ChatBotChatInterface.Document.SetText(TextSetOptions.None, node.Response);
                ChatBotChatInterface.IsReadOnly = true;

                currentNode = node;

                // If there are options left to explore
                if (node.Children != null && node.Children.Count > 0)
                {
                    CreateChatBotOptions(node.Children);
                }
                else
                {
                    ChatBotChatInterface.IsReadOnly = false;
                    ChatBotChatInterface.Document.SetText(TextSetOptions.None, "Error: Something went wrong. Please restart the chat.");
                    ChatBotChatInterface.IsReadOnly = true;
                }
            }
        }

        private void OnButtonClickChatBotKill(object sender, RoutedEventArgs e)
        {
            try
            {
                // Exit the page
                if (this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fatal error: {ex}");
            }
        }
    }
}