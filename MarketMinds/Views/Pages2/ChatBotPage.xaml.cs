using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.System;
using DomainLayer.Domain;
using MarketMinds.ViewModels;
using MarketMinds;
namespace Marketplace_SE
{
    public sealed partial class ChatBotPage : Page
    {
        private readonly ChatBotViewModel chatBotViewModel;
        private readonly HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

        public ChatBotPage()
        {
            this.InitializeComponent();
            chatBotViewModel = App.ChatBotViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void ChatBotOptionButton_Click(object sender, RoutedEventArgs eventArgs)
        {

        }

        private void OnButtonClickChatBotKill(object sender, RoutedEventArgs eventArgs)
        {
            var helpWindow = new Microsoft.UI.Xaml.Window();
            helpWindow.Content = new GetHelpPage();
            helpWindow.Activate();

            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void UserMessageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && !e.KeyStatus.IsMenuKeyDown && !e.KeyStatus.WasKeyDown)
            {
                SendUserMessage();
                e.Handled = true;
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            SendUserMessage();
        }

        private async void SendUserMessage()
        {
            string userMessage = UserMessageTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return;
            }

            Border userMessageBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15, 10, 15, 10),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 500,
                Margin = new Thickness(0, 5, 0, 5)
            };

            TextBlock userMessageText = new TextBlock
            {
                Text = userMessage,
                TextWrapping = TextWrapping.Wrap
            };

            userMessageBorder.Child = userMessageText;
            ChatMessagesPanel.Children.Add(userMessageBorder);

            UserMessageTextBox.Text = string.Empty;

            UserMessageTextBox.IsEnabled = false;
            SendMessageButton.IsEnabled = false;

            await Task.Delay(1000);

            string responseText;

            try
            {
                var request = new
                {
                    Message = userMessage
                };

                var requestJson = JsonSerializer.Serialize(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var apiUrl = "http://localhost:5000/api/chatbot";

                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var chatbotResponse = JsonSerializer.Deserialize<ChatbotResponse>(responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    responseText = chatbotResponse?.Message ?? "I'm sorry, I couldn't process that response.";
                }
                else
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    responseText = "I'm sorry, I'm having trouble processing your request right now. Please try again later.";
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    var httpEx = ex as HttpRequestException;
                    if (ex.Message.Contains("actively refused"))
                    {
                        responseText = "I can't connect to the server. Please make sure the API server is running on port 5000.";
                    }
                    else if (ex.Message.Contains("timed out"))
                    {
                        responseText = "The connection to the server timed out. Please try again later.";
                    }
                    else
                    {
                        responseText = "I'm sorry, I can't connect to the server right now. Please make sure the server is running and try again.";
                    }
                }
                else if (ex is TaskCanceledException)
                {
                    responseText = "The request timed out. Please try again later.";
                }
                else if (ex is JsonException)
                {
                    responseText = "I received an invalid response from the server. Please try again.";
                }
                else
                {
                    responseText = "I'm sorry, I'm having technical difficulties. Please try again later.";
                }
            }

            Border botResponseBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15, 10, 15, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 500,
                Margin = new Thickness(0, 5, 0, 5)
            };

            TextBlock botResponseText = new TextBlock
            {
                Text = responseText,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
            };

            botResponseBorder.Child = botResponseText;
            ChatMessagesPanel.Children.Add(botResponseBorder);

            UserMessageTextBox.IsEnabled = true;
            SendMessageButton.IsEnabled = true;

            UserMessageTextBox.Focus(FocusState.Programmatic);
        }

        private void UpdateChatUI()
        {
        }
    }
    public class ChatbotResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}