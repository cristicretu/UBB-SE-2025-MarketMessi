using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marketplace_SE.Services.DreamTeam;
using DomainLayer.Domain;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MarketMinds;

namespace Marketplace_SE.ViewModel.DreamTeam
{
    public partial class AccountPageViewModel : ObservableObject
    {
        private readonly AccountPageService _accountPageService;

        [ObservableProperty]
        private User? currentUser;

        [ObservableProperty]
        private ObservableCollection<UserOrder> orders;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        public AccountPageViewModel()
        {
            _accountPageService = App.AccountPageService;
            Orders = new ObservableCollection<UserOrder>();
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                CurrentUser = await _accountPageService.GetCurrentLoggedInUserAsync();

                if (CurrentUser != null)
                {
                    var fetchedOrders = await _accountPageService.GetUserOrdersAsync(CurrentUser.Id);
                    Orders.Clear();
                    if (fetchedOrders != null)
                    {
                        foreach (var order in fetchedOrders.OrderByDescending(o => o.Created))
                        {
                            Orders.Add(order);
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Could not load user information.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading account page data: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

