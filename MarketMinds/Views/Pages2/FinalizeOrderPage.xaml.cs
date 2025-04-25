using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Marketplace_SE.ViewModels;
using Marketplace_SE.Repositories;
using Marketplace_SE.Services;
using DataAccessLayer;
using MarketMinds;
using Microsoft.Extensions.Configuration;

namespace Marketplace_SE
{
    public sealed partial class FinalizeOrderPage : Page
    {
        public FinalizeOrderViewModel ViewModel { get; }

        public FinalizeOrderPage()
        {
            this.InitializeComponent();

            // Initialize the DataBaseConnection
            DataBaseConnection connection = App.DatabaseConnection;

            var ratingRepositoy = new RatingRepository(connection);
            var hardwareSurveyRepository = new HardwareSurveyRepository(connection);
            var loggerRepository = new LoggerRepository(connection);

            var ratingService = new RatingService(ratingRepositoy);
            var hardwareSurveyService = new HardwareSurveyService(hardwareSurveyRepository);
            var loggerService = new LoggerService(loggerRepository);
            ViewModel = new FinalizeOrderViewModel(ratingService, hardwareSurveyService, loggerService);

            // Set the DataContext for data binding
            this.DataContext = ViewModel;
        }

        private void OnButtonClickNavigateFinalizeOrderPageAccountPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMarketplacePage));
        }

        private async void OnButtonClickFinalizeOrder(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.FinalizeOrder();
                Frame.Navigate(typeof(MainMarketplacePage));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finalizing order: {ex.Message}");
                // Handle navigation or error display as needed
            }
        }
    }
}
