using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DomainLayer.Domain;
using MarketMinds;
using MarketMinds.Helpers.Selectors;
using MarketMinds.Services.ImagineUploadService;
using MarketMinds.ViewModels;
using Marketplace_SE.Data;
using Marketplace_SE.Utilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Marketplace_SE
{
    public sealed partial class ChatPage : Page
    {
        private ChatViewModel chatViewModel;
        private IImageUploadService imageUploadService;

        private User currentUser;
        private User targetUser;

        private DispatcherTimer updateTimer;
        private List<string> chatHistory = new();
        private ObservableCollection<Message> displayedMessages = new();

        private bool isInitializing = false;
        private bool initialLoadComplete = false;

        public ChatPage()
        {
            this.InitializeComponent();

            chatViewModel = App.ChatViewModel;
            imageUploadService = App.ImageUploadService;

            ChatListView.ItemsSource = displayedMessages;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
            displayedMessages.Clear();
            chatHistory.Clear();

            if (eventArgs.Parameter is UserNotSoldOrder selectedOrder)
            {
                currentUser = App.CurrentUser;
                targetUser = new User(selectedOrder.SellerId, "Seller", string.Empty);
            }
            else if (eventArgs.Parameter is User user)
            {
                currentUser = App.CurrentUser;
                targetUser = user;
            }
            else
            {
                currentUser = App.CurrentUser;
                targetUser = CreateDefaultTargetUser();
            }

            SetupTemplateSelector();

            TargetUserTextBlock.Text = $"Chatting with {targetUser.Username}";

            isInitializing = true;

            try
            {
                await chatViewModel.InitializeAsync(currentUser.Id);

                // Load initial messages
                await LoadInitialChatHistoryAsync();
                initialLoadComplete = true;

                // Start polling timer
                SetupUpdateTimer();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Chat initialization error", ex.Message);
            }
            finally
            {
                isInitializing = false;
            }
        }

        private User CreateDefaultTargetUser()
        {
            // Create a default target user (customer service or another default)
            return new User(
                99, // Use a default ID that won't conflict with real users
                "Customer Service",
                "support@marketplace.com");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedFrom(eventArgs);
            StopUpdateTimer();
        }

        private void SetupTemplateSelector()
        {
            var selector = new ChatMessageTemplateSelector
            {
                MyUserId = currentUser.Id,
                MyMessageTemplate = this.Resources["MyTextMessageTemplate"] as DataTemplate,
                OtherMessageTemplate = this.Resources["TargetTextMessageTemplate"] as DataTemplate
            };
            ChatListView.ItemTemplateSelector = selector;
        }

        private void SetupHardcodedUsers(int template)
        {
            switch (template)
            {
                case 0:
                    currentUser = new User(0, "test1", string.Empty);
                    targetUser = new User(1, "test2", string.Empty);
                    break;
                case 1:
                    currentUser = new User(1, "test2", string.Empty);
                    targetUser = new User(0, "test1", string.Empty);
                    break;
                case 2:
                    currentUser = new User(2, "test3", string.Empty);
                    targetUser = new User(3, "test4", string.Empty);
                    break;
                case 3: // Admin case
                    currentUser = new User(3, "test4", string.Empty); // Assuming ID 3 is admin
                    targetUser = new User(2, "test3", string.Empty);
                    break;
                default:
                    // Handle invalid template?
                    currentUser = App.CurrentUser;
                    targetUser = CreateDefaultTargetUser();
                    break;
            }
        }

        private void SetupUpdateTimer()
        {
            if (updateTimer == null)
            {
                updateTimer = new DispatcherTimer();
                updateTimer.Interval = TimeSpan.FromSeconds(2);
                updateTimer.Tick += UpdateTimer_Tick;
            }
            updateTimer.Start();
        }

        private void StopUpdateTimer()
        {
            if (updateTimer != null)
            {
                updateTimer.Stop();
                updateTimer.Tick -= UpdateTimer_Tick;
                updateTimer = null;
            }
        }

        private async void UpdateTimer_Tick(object sender, object e)
        {
            if (isInitializing || !initialLoadComplete)
            {
                return;
            }

            try
            {
                var messages = await chatViewModel.GetMessagesAsync();
                if (messages?.Count > 0)
                {
                    var existingIds = displayedMessages.Select(m => m.Id).ToHashSet();

                    foreach (var message in messages)
                    {
                        if (!existingIds.Contains(message.Id) && message.Id != 0)
                        {
                            AddMessageToDisplay(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for new messages: {ex.Message}");
            }
        }

        private async Task LoadInitialChatHistoryAsync()
        {
            var initialMessages = await chatViewModel.GetMessagesAsync();
            if (initialMessages != null)
            {
                foreach (var message in initialMessages)
                {
                    AddMessageToDisplay(message);
                }
            }
        }

        private void AddMessageToDisplay(Message message)
        {
            displayedMessages.Add(message);

            string timeString = DateTime.Now.ToString("[HH:mm]");
            bool isMe = message.UserId == currentUser.Id;
            string prefix = isMe ? "[You]" : "[Peer]";
            string contentForExport = message.Content;

            chatHistory.Add($"{timeString} {prefix}: {contentForExport}");
        }

        // --- Event Handlers ---
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string messageText = MessageBox.Text.Trim();
            if (string.IsNullOrEmpty(messageText) || chatViewModel == null || isInitializing)
            {
                return;
            }

            MessageBox.Text = string.Empty;
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;

            try
            {
                var message = await chatViewModel.SendMessageAsync(messageText);

                if (message != null)
                {
                    AddMessageToDisplay(message);
                }
                else
                {
                    ShowErrorDialog("Message error", "Failed to send message.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Message error", "Failed to send message: " + ex.Message);
            }
            finally
            {
                MessageBox.IsEnabled = true;
                SendButton.IsEnabled = true;
                MessageBox.Focus(FocusState.Programmatic);
            }
        }

        private async void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            if (chatViewModel == null || isInitializing)
            {
                return;
            }

            Window currentWindow = GetCurrentWindow();
            if (currentWindow == null)
            {
                return;
            }

            AttachButton.IsEnabled = false;

            try
            {
                string hexImageData = await imageUploadService.UploadImage(currentWindow);

                if (!string.IsNullOrEmpty(hexImageData))
                {
                    byte[] bytes = null;
                    try
                    {
                        bytes = DataEncoder.HexDecode(hexImageData);
                    }
                    catch (Exception ex)
                    {
                        AttachButton.IsEnabled = true;
                        return;
                    }

                    if (bytes != null && bytes.Length > 0)
                    {
                        try
                        {
                            var message = new Message
                            {
                                ConversationId = chatViewModel.CurrentConversationId,
                                Content = DataEncoder.HexEncode(bytes),
                                UserId = currentUser.Id
                            };

                            AddMessageToDisplay(message);
                            // Send the image content as a message
                            await chatViewModel.SendMessageAsync(DataEncoder.HexEncode(bytes));
                        }
                        catch (Exception ex)
                        {
                            ShowErrorDialog("Image send error", "Failed to send image message: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Image upload error", "Failed to upload image.");
            }
            finally
            {
                AttachButton.IsEnabled = true;
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var currentElement = sender as UIElement;
            if (currentElement == null)
            {
                return;
            }

            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"{currentUser.Username}_{targetUser.Username}_chat_history.txt"
            };
            savePicker.FileTypeChoices.Add("Text File", new List<string> { ".txt" });

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(currentElement.XamlRoot);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                try
                {
                    await Windows.Storage.FileIO.WriteLinesAsync(file, chatHistory);
                }
                catch (Exception ex)
                {
                    ShowErrorDialog("Export error", "Failed to export chat history.");
                    return;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StopUpdateTimer();
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(MainMarketplacePage));
            }
        }

        private async void ShowErrorDialog(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private Window GetCurrentWindow()
        {
            var currentWindow = Window.Current;
            if (currentWindow == null)
            {
                ShowErrorDialog("Window error", "Could not retrieve the current window.");
                return null;
            }
            return currentWindow;
        }
        private void ShowLoadingIndicator(bool show)
        {
            LoadingIndicator.IsActive = show;
            LoadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
