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

        private const int ZeroBytes = 0;  // magic numbers removal
        private const int ZeroMessages = 0;
        private const int ZeroMessageId = 0;
        private const int TimerOneSecond = 1;

        public ChatPage()
        {
            this.InitializeComponent();

            chatViewModel = App.ChatViewModel;
            imageUploadService = App.ImageUploadService;

            ChatListView.ItemsSource = displayedMessages;
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
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
                targetUser = App.TestingUser;
            }

            SetupTemplateSelector();

            TargetUserTextBlock.Text = $"Chatting with {targetUser.Username}";

            isInitializing = true;

            try
            {
                chatViewModel.InitializeChat(currentUser, targetUser);

                // Load initial messages
                LoadInitialChatHistory();
                initialLoadComplete = true;

                // Start polling timer
                SetupUpdateTimer();
            }
            catch (Exception chatInitializeException)
            {
                ShowErrorDialog("Chat initialization error", chatInitializeException.Message);
            }
            finally
            {
                isInitializing = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedFrom(navigationEventArgs);
            StopUpdateTimer();
        }

        private void SetupTemplateSelector()
        {
            var selector = new ChatMessageTemplateSelector
            {
                MyUserId = currentUser.Id,
                MyImageMessageTemplate = this.Resources["MyImageMessageTemplate"] as DataTemplate,
                MyTextMessageTemplate = this.Resources["MyTextMessageTemplate"] as DataTemplate,
                TargetImageMessageTemplate = this.Resources["TargetImageMessageTemplate"] as DataTemplate,
                TargetTextMessageTemplate = this.Resources["TargetTextMessageTemplate"] as DataTemplate
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
                    currentUser = null;
                    targetUser = null;
                    break;
            }
        }

        private void SetupUpdateTimer()
        {
            if (updateTimer == null)
            {
                updateTimer = new DispatcherTimer();
                updateTimer.Interval = TimeSpan.FromSeconds(TimerOneSecond);
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

        private void UpdateTimer_Tick(object sender, object eventArgs)
        {
            if (isInitializing || !initialLoadComplete)
            {
                return;
            }

            try
            {
                List<Message> newMessages = chatViewModel.CheckForNewMessages();
                if (newMessages?.Count > ZeroMessages)
                {
                    bool addedNew = false;
                    foreach (var message in newMessages)
                    {
                        if (!displayedMessages.Any(existingMessage => existingMessage.Id == message.Id && existingMessage.Id != ZeroMessageId))
                        {
                            AddMessageToDisplay(message);
                            addedNew = true;
                        }
                    }
                }
            }
            catch (Exception newMessagesException)
            {
                Debug.WriteLine($"Error checking for new messages: {newMessagesException.Message}");
            }
        }

        private void LoadInitialChatHistory()
        {
            List<Message> initialMessages = chatViewModel.GetInitialMessages();
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

            string timeString = DateTimeOffset.FromUnixTimeMilliseconds(message.Timestamp).ToString("[HH:mm]");
            bool isMe = message.Creator == currentUser.Id;
            string prefix = isMe ? "[You]" : "[Peer]";
            string contentForExport = message.ContentType == "text" ? message.Content : "<image>";

            chatHistory.Add($"{timeString} {prefix}: {contentForExport}");
        }

        // --- Event Handlers ---
        private void SendButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            string messageText = MessageBox.Text.Trim();
            if (string.IsNullOrEmpty(messageText) || chatViewModel == null || isInitializing)
            {
                return;
            }

            string textToSend = messageText;
            long pseudoTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            Message message = new Message
            {
                ConversationId = chatViewModel.GetConversation().Id,
                Content = textToSend,
                ContentType = "text",
                Creator = currentUser.Id,
                Timestamp = pseudoTimestamp
            };
            AddMessageToDisplay(message);
            MessageBox.Text = string.Empty;

            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;

            bool success = chatViewModel.SendTextMessage(textToSend);

            if (!success)
            {
                ShowErrorDialog("Message error", "Failed to send message.");
                return;
            }

            MessageBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            MessageBox.Focus(FocusState.Programmatic);
        }

        private async void AttachButton_Click(object sender, RoutedEventArgs routedEventArgs)
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
                    catch (Exception hexDecodeException)
                    {
                        AttachButton.IsEnabled = true;
                        return;
                    }

                if (bytes != null && bytes.Length > ZeroBytes)
                    {
                        long pseudoTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                        Message message = new Message
                        {
                            ConversationId = chatViewModel.GetConversation().Id,
                            Content = DataEncoder.HexEncode(bytes),
                            ContentType = "image",
                            Creator = currentUser.Id,
                            Timestamp = pseudoTimestamp
                        };
                        AddMessageToDisplay(message);

                        bool success = chatViewModel.SendImageMessage(bytes);

                        if (!success)
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception imageUploadingException)
            {
                ShowErrorDialog("Image upload error", "Failed to upload image.");
            }
            finally
            {
                AttachButton.IsEnabled = true;
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs routedEventArgs)
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
                catch (Exception chatHistoryExportException)
                {
                    ShowErrorDialog("Export error", "Failed to export chat history.");
                    return;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs routedEventArgs)
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
            LoadingIndicator.Visibility = Visibility.Visible;
        }
    }
}
