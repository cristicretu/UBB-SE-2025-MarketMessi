using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Marketplace_SE.Helpers;
using Marketplace_SE.Services;
using Microsoft.UI.Xaml.Controls;

namespace Marketplace_SE.ViewModels
{
    public class EnterIdPageViewModel : INotifyPropertyChanged
    {
        private readonly IUserService userService;
        private readonly Frame navigationFrame;
        private string enteredId = string.Empty;
        private string errorMessage = string.Empty;

        public string EnteredId
        {
            get => enteredId;
            set
            {
                enteredId = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand ContinueCommand { get; }
        public ICommand BackCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public EnterIdPageViewModel(IUserService userService, Frame navigationFrame)
        {
            this.userService = userService;
            this.navigationFrame = navigationFrame;
            ContinueCommand = new CustomCommand(ExecuteContinue, CanExecuteContinue);
            BackCommand = new CustomCommand(ExecuteBack, CanExecuteBack);
        }

        private void ExecuteContinue(object parameter)
        {
            var isValid = userService.ValidateUserId(EnteredId);
            if (isValid)
            {
                navigationFrame.Navigate(typeof(ResetPasswordPage));
            }
            else
            {
                ErrorMessage = "The ID you entered is not valid.";
            }
        }

        private void ExecuteBack(object parameter)
        {
            if (navigationFrame.CanGoBack)
            {
                navigationFrame.GoBack();
            }
        }

        private bool CanExecuteContinue(object parameter) => true;

        private bool CanExecuteBack(object parameter) => true;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
