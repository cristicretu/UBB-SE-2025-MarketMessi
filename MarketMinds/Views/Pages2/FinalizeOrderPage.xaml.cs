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
using System.Threading.Tasks;
using Marketplace_SE.Data;
using Marketplace_SE.HardwareSurvey;
using Marketplace_SE.Rating;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FinalizeOrderPage : Page
    {
        private RateAppComponent _rateAppComponent;
        private HardwareSurveyComponent _hardwareSurveyComponent;

        public FinalizeOrderPage()
        {
            this.InitializeComponent();

            // Initialize database connection if not already initialized
            if (Database.database == null)
            {
                try
                {
                    // Create database connection 
                    Database.database = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
                    bool connected = Database.database.Connect();

                    if (connected)
                    {
                        System.Diagnostics.Debug.WriteLine("Database connected successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to connect to database");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Database connection error: {ex.Message}");
                }
            }


            // Create placeholder services (will be replaced with real implementations)
            var ratingService = new DatabaseRatingService();
            var hardwareSurveyService = new DatabaseHardwareSurveyService();
            var loggerService = new DatabaseLoggerService();

            // Initialize components
            _rateAppComponent = new RateAppComponent(ratingService);
            _hardwareSurveyComponent = new HardwareSurveyComponent(hardwareSurveyService, loggerService);

            // Register for page loading event to ensure XamlRoot is set
            Loaded += FinalizeOrderPage_Loaded;

            Database.database.Close();
        }

        private void FinalizeOrderPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set XamlRoot for dialog components
            _rateAppComponent.SetXamlRoot(this.XamlRoot);
            _hardwareSurveyComponent.SetXamlRoot(this.XamlRoot);
        }

        private void OnButtonClickNavigateFinalizeOrderPageAccountPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMarketplacePage));
        }

        private async void OnButtonClickFinalizeOrder(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set XamlRoot again just before showing dialogs
                //// System.Diagnostics.Debug.WriteLine("Setting XamlRoot");
                _rateAppComponent.SetXamlRoot(this.XamlRoot);

                // First show rating and survey dialogs
                //// System.Diagnostics.Debug.WriteLine("Starting rating and survey dialogs");

                // Debug the XamlRoot value
                //// System.Diagnostics.Debug.WriteLine($"XamlRoot is null: {this.XamlRoot == null}");

                // Show more detailed debug for each step
                //// System.Diagnostics.Debug.WriteLine("Calling OnTransactionCompleted");
                _rateAppComponent.OnTransactionCompleted();

                //// System.Diagnostics.Debug.WriteLine("Calling CheckAndShowRatingPromptAsync");
                bool ratingShown = await _rateAppComponent.CheckAndShowRatingPromptAsync();
                //// System.Diagnostics.Debug.WriteLine($"Rating dialog shown: {ratingShown}");

                // After rating dialog closes, show hardware survey using the explicit XamlRoot method
                //// System.Diagnostics.Debug.WriteLine("Calling ShowWithExplicitXamlRoot");
                bool surveyShown = await _hardwareSurveyComponent.ShowWithExplicitXamlRoot(this.XamlRoot);
                //// System.Diagnostics.Debug.WriteLine($"Hardware survey shown: {surveyShown}");

                // Then navigate back to marketplace
                //// System.Diagnostics.Debug.WriteLine("Navigating to marketplace");
                Frame.Navigate(typeof(MainMarketplacePage));
            }
            catch (Exception ex)
            {
                //// System.Diagnostics.Debug.WriteLine($"Error in FinalizeOrder: {ex.Message}");
                //// System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Still navigate to maintain functionality
                // Add a small delay before navigation
                await Task.Delay(500);
                //// System.Diagnostics.Debug.WriteLine("Navigating to marketplace");
                Frame.Navigate(typeof(MainMarketplacePage));
            }
        }

        // Database-connected services for ratings and hardware survey
        private class DatabaseRatingService : IRatingDatabaseService
        {
            public async Task SaveRatingAsync(RatingData ratingData)
            {
                try
                {
                    if (Database.database != null)
                    {
                        // Prepare the SQL query
                        string query = @"
                            INSERT INTO Ratings (UserID, Rating, Comment, Timestamp, AppVersion)
                            VALUES (@UserID, @Rating, @Comment, @Timestamp, @AppVersion)";

                        // Prepare parameters
                        string[] args = new string[] { "@UserID", "@Rating", "@Comment", "@Timestamp", "@AppVersion" };
                        object[] values = new object[] {
                            ratingData.UserID,
                            ratingData.Rating,
                            ratingData.Comment ?? (object)DBNull.Value,
                            ratingData.Timestamp,
                            ratingData.AppVersion
                        };

                        Database.database = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
                        bool connected = Database.database.Connect();

                        // Execute the query
                        int result = Database.database.Execute(query, args, values);

                        Database.database.Close();
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue
                    System.Diagnostics.Debug.WriteLine($"Error saving rating to database: {ex.Message}");
                }
            }
        }

        private class DatabaseHardwareSurveyService : IHardwareSurveyDatabaseService
        {
            public async Task SaveHardwareDataAsync(HardwareData hardwareData)
            {
                try
                {
                    if (Database.database != null)
                    {
                        // Prepare the SQL query
                        string query = @"
                            INSERT INTO HardwareSurvey (
                                DeviceID, DeviceType, OperatingSystem, OSVersion, 
                                BrowserName, BrowserVersion, ScreenResolution, 
                                AvailableRAM, CPUInformation, GPUInformation, 
                                ConnectionType, Timestamp, AppVersion
                            )
                            VALUES (
                                @DeviceID, @DeviceType, @OperatingSystem, @OSVersion,
                                @BrowserName, @BrowserVersion, @ScreenResolution,
                                @AvailableRAM, @CPUInformation, @GPUInformation,
                                @ConnectionType, @Timestamp, @AppVersion
                            )";

                        // Prepare parameters
                        string[] args = new string[] {
                            "@DeviceID", "@DeviceType", "@OperatingSystem", "@OSVersion",
                            "@BrowserName", "@BrowserVersion", "@ScreenResolution",
                            "@AvailableRAM", "@CPUInformation", "@GPUInformation",
                            "@ConnectionType", "@Timestamp", "@AppVersion"
                        };

                        object[] values = new object[] {
                            hardwareData.DeviceID,
                            hardwareData.DeviceType,
                            hardwareData.OperatingSystem,
                            hardwareData.OSVersion,
                            hardwareData.BrowserName ?? (object)DBNull.Value,
                            hardwareData.BrowserVersion ?? (object)DBNull.Value,
                            hardwareData.ScreenResolution,
                            hardwareData.AvailableRAM,
                            hardwareData.CPUInformation ?? (object)DBNull.Value,
                            hardwareData.GPUInformation ?? (object)DBNull.Value,
                            hardwareData.ConnectionType,
                            hardwareData.Timestamp,
                            hardwareData.AppVersion
                        };

                        Database.database = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
                        bool connected = Database.database.Connect();

                        // Execute the query
                        int result = Database.database.Execute(query, args, values);

                        // Print data to console for demonstration
                        System.Diagnostics.Debug.WriteLine("========== HARDWARE SURVEY DATA SAVED TO DATABASE ==========");
                        System.Diagnostics.Debug.WriteLine($"Device Type: {hardwareData.DeviceType}");
                        System.Diagnostics.Debug.WriteLine($"OS: {hardwareData.OperatingSystem} {hardwareData.OSVersion}");
                        System.Diagnostics.Debug.WriteLine($"Screen: {hardwareData.ScreenResolution}");
                        System.Diagnostics.Debug.WriteLine($"RAM: {hardwareData.AvailableRAM}");
                        System.Diagnostics.Debug.WriteLine($"CPU: {hardwareData.CPUInformation}");
                        System.Diagnostics.Debug.WriteLine($"GPU: {hardwareData.GPUInformation}");
                        System.Diagnostics.Debug.WriteLine($"Connection: {hardwareData.ConnectionType}");

                        Database.database.Close();
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue
                    System.Diagnostics.Debug.WriteLine($"Error saving hardware survey to database: {ex.Message}");
                }
            }
        }

        private class DatabaseLoggerService : ILoggerService
        {
            public void LogInfo(string message)
            {
                System.Diagnostics.Debug.WriteLine($"INFO: {message}");
            }

            public void LogError(string message)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {message}");
            }
        }
    }
}
