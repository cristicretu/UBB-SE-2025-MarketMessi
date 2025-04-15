using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Marketplace_SE.Data;
using Marketplace_SE.Objects;
using Marketplace_SE.Utilities;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatPage : Page
    {

        private List<string> chatHistory = new();


        private Conversation conversation;
        private long lastMessageTimestamp = 0;
        private User me;
        private User target;

        private Task loopUpdater;
        private CancellationTokenSource cancellationTokenSource;

        public ChatPage(int hardcoded_template=0)
        {
            // Open database connection
            // Read Database.cs for guide
            Database.database = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            bool status = Database.database.Connect();

            if (!status)
            {
                //database connection failed
                //ShowDialog("Database connection error", "Error connecting to database");
                
                Notification notification = new Notification("Database connection error", "Error connecting to database");
                notification.OkButton.Click += (s, e) =>
                {
                    notification.GetWindow().Close();
                    Database.database.Close();
                    if (hardcoded_template == 0 || hardcoded_template == 1 || hardcoded_template == 2)
                    {
                        Frame.Navigate(typeof(MainMarketplacePage));
                    }
                    if (hardcoded_template == 3)
                    {
                        Frame.Navigate(typeof(AdminAccountPage));
                    }
                };
                notification.GetWindow().Activate();
                return;
            }

            //hardcoded users

            if (hardcoded_template == 0)
            {
                this.me = new User("test1", "");
                this.me.SetId(0);

                this.target = new User("test2", "");
                this.target.SetId(1);
            }
            if(hardcoded_template == 1)
            {
                this.me = new User("test2", "");
                this.me.SetId(1);

                this.target = new User("test1", "");
                this.target.SetId(0);
            }
            if(hardcoded_template == 2)
            {
                this.me = new User("test3", "");
                this.me.SetId(2);

                this.target = new User("test4", "");
                this.target.SetId(3);
            }
            if (hardcoded_template == 3)
            {
                this.me = new User("test4", "");
                this.me.SetId(3);

                this.target = new User("test3", "");
                this.target.SetId(2);
            }

            this.InitializeComponent();

            //Database actions
            var data = Database.database.Get("SELECT * FROM dbo.Conversations WHERE ((user1=@MyID AND user2=@TargetID) OR (user2=@MyID AND user1=@TargetID))",
            new string[]
            {
                "@MyID",
                "@TargetID"
            }, new object[]
            {
                me.id,
                target.id
            });

            List<Conversation> conversationList = Database.database.ConvertToObject<Conversation>(data);

            if (conversationList.Count == 0)
            {
                //Create conversation

                int affected = Database.database.Execute("INSERT INTO Conversations (user1, user2) VALUES (@MyID, @TargetID)",
                    new string[]
                    {
                        "@MyID",
                        "@TargetID"
                    }, new object[]
                    {
                        me.id,
                        target.id
                    }
                );

                //Get again
                data = Database.database.Get("SELECT * FROM Conversations WHERE ((user1=@MyID AND user2=@TargetID) OR (user2=@MyID AND user1=@TargetID))",
                new string[]
                {
                    "@MyID",
                    "@TargetID"
                }, new object[]
                {
                    me.id,
                    target.id
                });
                conversationList = Database.database.ConvertToObject<Conversation>(data);
            }

            conversation = conversationList[0];

            // Load chat history from conv 0
            data = Database.database.Get("SELECT * FROM dbo.Messages WHERE conversationId=@ConvID",
            new string[]
            {
                "@ConvID",
            }, new object[]
            {
                conversation.id
            });

            List<Message> messages = Database.database.ConvertToObject<Message>(data);
            //Sort messages timestamp
            messages.Sort((a, b) =>
            {
                return (int)(a.timestamp - b.timestamp);
            });

            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].creator != me.id)
                {
                    lastMessageTimestamp = messages[i].timestamp;
                }

                if (messages[i].contentType == "text")
                {
                    AddMessageToChat(messages[i].content, me.id == messages[i].creator);
                }
                else if (messages[i].contentType == "image")
                {
                    byte[] imgBytes = DataEncoder.HexDecode(messages[i].content);
                    DisplayImageFromBytes(imgBytes, me.id == messages[i].creator);
                }
                else
                {
                    //must not happen
                }
            }

            Debug.WriteLine(lastMessageTimestamp.ToString());

            cancellationTokenSource = new CancellationTokenSource();
            loopUpdater = Task.Run(() => UpdateLoop(cancellationTokenSource.Token), cancellationTokenSource.Token);


        }



        public void StopUpdateLoop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        private void UpdateLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Update conversation with new messages

                var data = Database.database.Get("SELECT * FROM dbo.Messages WHERE conversationId=@ConvID AND creator!=@MyID AND timestamp>@LastMessageTimestamp",
                new string[]
                {
                    "@ConvID",
                    "@MyID",
                    "@LastMessageTimestamp"
                }, new object[]
                {
                    conversation.id,
                    me.id,
                    lastMessageTimestamp

                });

                List<Message> messages = Database.database.ConvertToObject<Message>(data);
                messages.Sort((a, b) =>
                {
                    return (int)(a.timestamp - b.timestamp);
                });

                for (int i = 0; i < messages.Count; i++)
                {
                    lastMessageTimestamp = messages[i].timestamp;

                    if (messages[i].contentType == "text")
                    {
                        int idx = i;
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            AddMessageToChat(messages[idx].content, me.id == messages[idx].creator);
                        });
                    }
                    else if (messages[i].contentType == "image")
                    {
                        int idx = i;
                        byte[] imgBytes = DataEncoder.HexDecode(messages[idx].content);
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            DisplayImageFromBytes(imgBytes, me.id == messages[idx].creator);
                        });
                    }
                    else
                    {
                        //must not happen
                    }
                }

                try
                {
                    Task.Delay(1000, token).Wait();
                }
                catch (Exception)
                {
                    break;
                }

            }
        }



        private void SendMsgDatabase(object _content, string contentType = "")
        {
            //Allowed content type
            // text
            // image


            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();


            string content = "";
            if (_content.GetType() == typeof(string))
            {
                if (contentType == "")
                    contentType = "text";
                content = _content as string;
            }
            else
            {
                if (_content.GetType() != typeof(byte[]))
                {
                    //only byte[] accepted
                    return;
                }
                content = DataEncoder.HexEncode((byte[])_content);
                if (contentType == "")
                    contentType = "bytes";
            }

            int affected = Database.database.Execute("INSERT INTO Messages (conversationId, creator,timestamp,contentType,content) VALUES (@ConvID, @MyID,@Timestamp,@ContentType,@Content)",
                    new string[]
                    {
                        "@ConvID",
                        "@MyID",
                        "@Timestamp",
                        "@ContentType",
                        "@Content"
                    }, new object[]
                    {
                        conversation.id,
                        me.id,
                        unixTime,
                        contentType,
                        content
                    }
                );
        }


        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            SendMsgDatabase(message);
            AddMessageToChat(message, true);
            MessageBox.Text = "";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StopUpdateLoop();
            Database.database.Close();
            if(this.me.id == 0 || this.me.id == 1 || this.me.id == 2)
            {
                Frame.Navigate(typeof(MainMarketplacePage));
            }
            if (this.me.id == 3)
            {
                Frame.Navigate(typeof(AdminAccountPage));
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "ChatHistory";

            // Required for WinUI 3 desktop apps
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await Windows.Storage.FileIO.WriteLinesAsync(file, chatHistory);
            }
        }


        private void AddMessageToChat(string message, bool isClient)
        {
            string timeStamp = DateTime.Now.ToString("[HH:mm]");
            string displayText = $"{timeStamp} {message}";

            var textBlock = new TextBlock
            {
                Text = displayText,
                TextWrapping = TextWrapping.WrapWholeWords,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
            };

            var border = new Border
            {
                Child = textBlock,
                Background = new SolidColorBrush(isClient ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 4, 0, 4),
                Padding = new Thickness(8),
                HorizontalAlignment = isClient ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 250
            };

            ChatPanel.Children.Add(border);

            string prefix = isClient ? "[You]" : "[Peer]";
            chatHistory.Add($"{timeStamp} {prefix}: {message}");

            // Auto-scroll to bottom
            if (ChatPanel.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
            }
        }


        /*
        private void SendMessage(string message)
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", sendPort); // Send to server
                byte[] data = Encoding.UTF8.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    AddMessageToChat("Send failed: " + ex.Message, false);
                });
            }
        }
        */

        private async void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);
                byte[] bytes = buffer.ToArray();

                SendMsgDatabase(bytes, "image");

                // Tag the image so receiver knows what it is
        /*  
        byte[] header = Encoding.UTF8.GetBytes("IMG|");
                byte[] fullMessage = header.Concat(bytes).ToArray();

                SendBytes(fullMessage);
        */
                DisplayImageFromBytes(bytes, isClient: true);
                chatHistory.Add($"[You]: <sent image>");
            }
        }


        private void DisplayImageFromBytes(byte[] imageData, bool isClient)
        {
            using var stream = new MemoryStream(imageData);
            var bitmap = new BitmapImage();
            stream.Position = 0;

            bitmap.SetSource(stream.AsRandomAccessStream());

            var image = new Image
            {
                Source = bitmap,
                Width = 150,
                Height = 150,
                Stretch = Stretch.UniformToFill
            };

            var border = new Border
            {
                Child = image,
                Background = new SolidColorBrush(isClient ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 4, 0, 4),
                Padding = new Thickness(4),
                HorizontalAlignment = isClient ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            ChatPanel.Children.Add(border);
        }

    }
}
