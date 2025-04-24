using Marketplace_SE.Services;
using Marketplace_SE.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Marketplace_SE
{
    public sealed partial class EnterIdPage : Page
    {
        public EnterIdPageViewModel ViewModel { get; }

        public EnterIdPage()
        {
            this.InitializeComponent();
            IUserService userService = new UserService();
            ViewModel = new EnterIdPageViewModel(userService, this.Frame); // Pass the Frame instance
            DataContext = ViewModel;
        }
    }
}
