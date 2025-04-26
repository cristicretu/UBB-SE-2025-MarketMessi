using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DomainLayer.Domain;
using MarketMinds.Services;
using Marketplace_SE.Utilities;

namespace MarketMinds.ViewModels
{
    public class AccountPageViewModel : INotifyPropertyChanged
    {
        private User _currentUser;
        private ObservableCollection<UserOrder> _orders;
        private UserOrder _selectedOrder;
        private string _errorMessage;
        private bool _isLoading;
        private readonly IAccountPageService _accountPageService;

        public event PropertyChangedEventHandler PropertyChanged;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<UserOrder> Orders
        {
            get => _orders;
            set
            {
                if (_orders != value)
                {
                    _orders = value;
                    OnPropertyChanged();
                }
            }
        }

        public UserOrder SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand LoadDataCommand { get; private set; }
        public RelayCommand ViewOrderCommand { get; private set; }
        public RelayCommand ReturnItemCommand { get; private set; }
        public RelayCommand NavigateToMainCommand { get; private set; }

        public AccountPageViewModel(IAccountPageService accountPageService)
        {
            _accountPageService = accountPageService ?? throw new ArgumentNullException(nameof(accountPageService));
            Orders = new ObservableCollection<UserOrder>();

            // Initialize commands
            LoadDataCommand = new RelayCommand(async _ => await LoadUserDataAsync());
            ViewOrderCommand = new RelayCommand(_ => ViewOrderDetails());
            ReturnItemCommand = new RelayCommand(_ => ReturnItem(), CanReturnItem);
            NavigateToMainCommand = new RelayCommand(_ => NavigateToMainPage());
        }

        public async Task LoadUserDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                CurrentUser = await _accountPageService.GetCurrentLoggedInUserAsync();

                if (CurrentUser != null)
                {
                    var userOrders = await _accountPageService.GetUserOrdersAsync(CurrentUser.Id);
                    Orders.Clear();

                    if (userOrders != null && userOrders.Any())
                    {
                        foreach (var order in userOrders)
                        {
                            // Add additional order properties if needed
                            // For example, set the order status based on properties or calculate display values
                            order.OrderStatus = DetermineOrderStatus(order);

                            Orders.Add(order);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading account data: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading account page: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string DetermineOrderStatus(UserOrder order)
        {
            // Logic to determine order status - this is a placeholder
            // Replace with actual business logic in your application
            return "Completed"; // Default status
        }

        private void ViewOrderDetails()
        {
            if (SelectedOrder != null)
            {
                // Logic to navigate to order details page
                // This would be handled in the code-behind or through navigation service
            }
        }

        private void ReturnItem()
        {
            if (SelectedOrder != null)
            {
                // Logic to navigate to return item page
                // This would be handled in the code-behind or through navigation service
            }
        }

        private bool CanReturnItem(object parameter)
        {
            // Only allow return if this is a buy order for the current user
            return SelectedOrder != null && CurrentUser != null && SelectedOrder.BuyerId == CurrentUser.Id;
        }

        private void NavigateToMainPage()
        {
            // Navigation logic
            // This would be handled in the code-behind or through navigation service
        }

        public string FormatOrderDateTime(ulong timestamp)
        {
            try
            {
                return DataEncoder.ConvertTimestampToLocalDateTime(timestamp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting timestamp: {ex.Message}");
                return DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Simple implementation of RelayCommand for MVVM pattern
    public class RelayCommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
