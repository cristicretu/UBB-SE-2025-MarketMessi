using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Marketplace_SE.Data;
using DomainLayer.Domain;
using Marketplace_SE.Utilities;
using MarketMinds.ViewModels;
using MarketMinds;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.IdentityModel.Tokens;
using MarketMinds.Helpers.Selectors;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        private ChatViewModel chatViewModel;

        private User me;
        private User target;

        private DispatcherTimer updateTimer;
        private List<string> chatHistory = new();
        private ObservableCollection<Message> displayedMessages = new();

        private bool isInitializing = false;
        private bool initialLoadComplete = false;

        public ChatPage()
        {
            this.InitializeComponent();

            chatViewModel = App.ChatViewModel;

            ChatListView.ItemsSource = displayedMessages;
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
            displayedMessages.Clear();
            chatHistory.Clear();

            int hardcoded_template = eventArgs.Parameter is int template ? template : 0;
            SetupHardcodedUsers(hardcoded_template);

            if (me == null || target == null)
            {
                ShowErrorAndNavigateBack("User setup error", "Could not determine users for chat.");
                return;
            }

            SetupTemplateSelector();

            TargetUserTextBlock.Text = $"Chatting with {target.Username}";

            isInitializing = true;

            try
            {
                chatViewModel.InitializeChat(me, target);

                // Load initial messages
                LoadInitialChatHistory();
                initialLoadComplete = true;

                // Start polling timer
                SetupUpdateTimer();
            }
            catch (Exception ex)
            {
                ShowErrorAndNavigateBack("Chat initialization error", ex.Message);
            }
            finally
            {
                isInitializing = false;
            }
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
                MyUserId = me.Id,
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
                    me = new User(0, "test1", string.Empty);
                    target = new User(1, "test2", string.Empty);
                    break;
                case 1:
                    me = new User(1, "test2", string.Empty);
                    target = new User(0, "test1", string.Empty);
                    break;
                case 2:
                    me = new User(2, "test3", string.Empty);
                    target = new User(3, "test4", string.Empty);
                    break;
                case 3: // Admin case
                    me = new User(3, "test4", string.Empty); // Assuming ID 3 is admin
                    target = new User(2, "test3", string.Empty);
                    break;
                default:
                    // Handle invalid template?
                    me = null;
                    target = null;
                    break;
            }
        }

        private void SetupUpdateTimer()
        {
            if (updateTimer == null)
            {
                updateTimer = new DispatcherTimer();
                updateTimer.Interval = TimeSpan.FromSeconds(1);
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

        private void UpdateTimer_Tick(object sender, object e)
        {
            if (isInitializing || !initialLoadComplete)
            {
                return;
            }

            try
            {
                List<Message> newMessages = chatViewModel.CheckForNewMessages();
                if (newMessages?.Count > 0)
                {
                    bool addedNew = false;
                    foreach (var message in newMessages)
                    {
                        if (!displayedMessages.Any(m => m.Id == message.Id && m.Id != 0))
                        {
                            AddMessageToDisplay(message);
                            addedNew = true;
                        }
                    }
                    if (addedNew)
                    {
                        ScrollChatToBottom();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for new messages: {ex.Message}");
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
            ScrollChatToBottom();
        }

        private void AddMessageToDisplay(Message message)
        {
            displayedMessages.Add(message);

            string timeString = DateTimeOffset.FromUnixTimeMilliseconds(message.Timestamp).ToString("[HH:mm]");
            bool isMe = message.Creator == me.Id;
            string prefix = isMe ? "[You]" : "[Peer]";
            string contentForExport = message.ContentType == "text" ? message.Content : "<image>";

            chatHistory.Add($"{timeString} {prefix}: {contentForExport}");
        }

        private void ScrollChatToBottom()
        {
            if (displayedMessages.Count > 0)
            {
                var lastItem = displayedMessages.Last();
                ChatListView.ScrollIntoView(displayedMessages[displayedMessages.Count - 1]);
            }
        }

        // --- Event Handlers ---
        private void SendButton_Click(object sender, RoutedEventArgs e)
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
                Creator = me.Id,
                Timestamp = pseudoTimestamp
            };
            AddMessageToDisplay(message);
            ScrollChatToBottom();
            MessageBox.Text = string.Empty;

            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;

            bool success = chatViewModel.SendTextMessage(textToSend);

            if (!success)
            {
                ShowErrorAndNavigateBack("Message error", "Failed to send message.");
                return;
            }

            MessageBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            MessageBox.Focus(FocusState.Programmatic);
        }

        private async void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            if (chatViewModel == null || isInitializing)
            {
                return;
            }
            var currentElement = sender as UIElement;
            if (currentElement == null)
            {
                return;
            }

            var picker = new Windows.Storage.Pickers.FileOpenPicker { SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(currentElement.XamlRoot);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, windowHandle);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                AttachButton.IsEnabled = false;
                byte[] bytes = null;
                try
                {
                    var buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);
                    bytes = buffer.ToArray();
                }
                catch (Exception ex)
                {
                    AttachButton.IsEnabled = true;
                    return;
                }
                if (bytes != null)
                {
                    long pseudoTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    string hexContent = DataEncoder.HexEncode(bytes);

                    var message = new Message
                    {
                        ConversationId = chatViewModel.GetConversation().Id,
                        Content = hexContent,
                        ContentType = "image",
                        Creator = me.Id,
                        Timestamp = pseudoTimestamp
                    };

                    AddMessageToDisplay(message);
                    ScrollChatToBottom();

                    bool success = chatViewModel.SendImageMessage(bytes);
                    if (!success)
                    {
                        ShowErrorAndNavigateBack("Image error", "Failed to send image.");
                        return;
                    }
                }
                AttachButton.IsEnabled = true;
            }
        }

        private async Task ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var currentElement = sender as UIElement;
            if (currentElement == null)
            {
                return;
            }

            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"{me.Username}_{target.Username}_chat_history.txt"
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
                    ShowErrorAndNavigateBack("Export error", "Failed to export chat history.");
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

        private void ShowErrorAndNavigateBack(string title, string message)
        {
            StopUpdateTimer();
            var dialog = new ContentDialog { Title = title, Content = message, CloseButtonText = "OK", XamlRoot = this.Content.XamlRoot };
            dialog.Closed += (s, args) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                else
                {
                    Frame.Navigate(typeof(MainMarketplacePage));
                }
            };
            dialog.ShowAsync();
        }

        private void ShowLoadingIndicator(bool show)
        {
            LoadingIndicator.IsActive = show;
            LoadingIndicator.Visibility = Visibility.Visible;
        }
    }
}
