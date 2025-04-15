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
using Marketplace_SE.Objects;
using Marketplace_SE.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminFindCallPage : Page
    {
        public AdminFindCallPage()
        {
            this.InitializeComponent();

            //Lookup the wait list and try to open a chat
        }
        private void OnButtonClickNavigateAdminFindCallPageAdminAccountPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdminAccountPage));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FrameNavigation.NavigateWithConstructorParameters<ChatPage>(this.Frame, 3);
        }
    }
}
