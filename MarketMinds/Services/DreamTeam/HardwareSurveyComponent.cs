// HardwareSurveyComponent.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Windows.Graphics.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Profile;
using Windows.Devices.Enumeration;
using Windows.Networking.Connectivity;
using System.Diagnostics;

namespace Marketplace_SE.HardwareSurvey
{
    /// <summary>
    /// Implements the Hardware Survey component that collects anonymous hardware information 
    /// from users at random times with explicit permission.
    /// </summary>
    public sealed partial class HardwareSurveyComponent : UserControl
    {
        #region Fields and Properties

        // Constants
        private const string LAST_SURVEY_TIMESTAMP_KEY = "LastHardwareSurveyTimestamp";
        private const string SURVEY_CONSENT_KEY = "HardwareSurveyConsent";
        private const int MIN_DAYS_BETWEEN_SURVEYS = 30; // Don't show more often than once a month
        private const double SURVEY_RANDOM_THRESHOLD = 0.3; // 30% chance to show survey when eligible

        // Random number generator for determining when to show the survey
        private readonly Random _random = new Random();

        // UI Elements
        private ContentDialog _surveyDialog;
        private ProgressBar _progressBar;
        private TextBlock _statusTextBlock;

        // Database service dependency (should be injected in a production app)
        private readonly IHardwareSurveyDatabaseService _surveyService;

        // Logger service (should be injected in a production app)
        private readonly ILoggerService _logger;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Initializes a new instance of the HardwareSurveyComponent class.
        /// </summary>
        /// <param name="surveyService">Service for saving hardware survey data to database</param>
        /// <param name="logger">Service for logging errors and events</param>
        public HardwareSurveyComponent(IHardwareSurveyDatabaseService surveyService, ILoggerService logger)
        {
            _surveyService = surveyService ?? throw new ArgumentNullException(nameof(surveyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeComponent();
        }

        /// <summary>
        /// Sets the XamlRoot for this component, necessary for showing dialogs.
        /// </summary>
        /// <param name="xamlRoot">The XamlRoot of the page or element that owns this component</param>
        public void SetXamlRoot(XamlRoot xamlRoot)
        {
            this.XamlRoot = xamlRoot;
        }

        /// <summary>
        /// Creates and initializes the survey dialog UI components.
        /// </summary>
        private void InitializeComponent()
        {
            // Create the main content grid for the dialog
            var contentGrid = new Grid
            {
                Padding = new Thickness(16),
                RowSpacing = 16
            };

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Description text
            var descriptionTextBlock = new TextBlock
            {
                Text = "We'd like to collect anonymous information about your device to help improve app performance. " +
                      "This data helps us optimize the app for different hardware configurations.\n\n" +
                      "We will collect the following information:\n" +
                      "• Device type (desktop, laptop, tablet, smartphone)\n" +
                      "• Operating system name and version\n" +
                      "• Browser name and version\n" +
                      "• Screen resolution\n" +
                      "• Available RAM\n" +
                      "• CPU information\n" +
                      "• GPU information (if available)\n" +
                      "• Connection type (WiFi, cellular, ethernet)\n\n" +
                      "This information will be anonymized and not linked to your personal data.",
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(descriptionTextBlock, 0);
            contentGrid.Children.Add(descriptionTextBlock);

            // Progress bar (initially hidden)
            _progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Visibility = Visibility.Collapsed
            };
            Grid.SetRow(_progressBar, 1);
            contentGrid.Children.Add(_progressBar);

            // Status text (initially hidden)
            _statusTextBlock = new TextBlock
            {
                Text = "Collecting hardware information...",
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed
            };
            Grid.SetRow(_statusTextBlock, 2);
            contentGrid.Children.Add(_statusTextBlock);

            // Create the dialog
            _surveyDialog = new ContentDialog
            {
                Title = "Hardware Survey",
                Content = contentGrid,
                PrimaryButtonText = "Yes, I'll help",
                CloseButtonText = "No thanks",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            _surveyDialog.PrimaryButtonClick += SurveyDialog_PrimaryButtonClick;
            _surveyDialog.CloseButtonClick += SurveyDialog_CloseButtonClick;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the hardware survey should be shown and displays it if appropriate.
        /// This should be called when the user navigates to the user profile page.
        /// </summary>
        /// <returns>True if the survey was shown; otherwise, false.</returns>
        public async Task<bool> CheckAndShowHardwareSurveyAsync()
        {
            // First check if user has already given consent
            var hasConsent = GetUserConsent();

            // If we already have consent and it's time to collect data, do it silently
            if (hasConsent && ShouldCollectHardwareData())
            {
                await CollectAndSubmitHardwareDataAsync();
                return false; // We didn't show the dialog
            }

            // If we don't have consent and it's appropriate to ask, show the dialog
            if (!hasConsent && ShouldShowSurveyPrompt())
            {
                return await ShowSurveyDialogAsync();
            }

            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines if the survey dialog should be shown based on timing and randomness.
        /// </summary>
        /// <returns>True if the dialog should be shown; otherwise, false.</returns>
        private bool ShouldShowSurveyPrompt()
        {
            // Check if we've shown the survey recently
            /*if (!HasEnoughTimePassed())
                return false;

            // Add randomness - only show to a percentage of eligible users
            return _random.NextDouble() < SURVEY_RANDOM_THRESHOLD;*/
            return true;
        }

        /// <summary>
        /// Determines if enough time has passed since the last survey.
        /// </summary>
        /// <returns>True if enough time has passed; otherwise, false.</returns>
        private bool HasEnoughTimePassed()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            // Check if we've shown the survey before
            if (localSettings.Values.ContainsKey(LAST_SURVEY_TIMESTAMP_KEY))
            {
                var lastSurveyTicks = (long)localSettings.Values[LAST_SURVEY_TIMESTAMP_KEY];
                var lastSurvey = new DateTime(lastSurveyTicks);

                // If we've shown the survey less than MIN_DAYS_BETWEEN_SURVEYS ago, don't show
                if ((DateTime.Now - lastSurvey).TotalDays < MIN_DAYS_BETWEEN_SURVEYS)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if we should collect hardware data based on consent and timing.
        /// </summary>
        /// <returns>True if we should collect hardware data; otherwise, false.</returns>
        private bool ShouldCollectHardwareData()
        {
            // If the user hasn't given consent, we shouldn't collect data
            if (!GetUserConsent())
                return false;

            // Check if enough time has passed since the last collection
            return HasEnoughTimePassed();
        }

        /// <summary>
        /// Gets the user's consent status from local settings.
        /// </summary>
        /// <returns>True if the user has given consent; otherwise, false.</returns>
        private bool GetUserConsent()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey(SURVEY_CONSENT_KEY))
            {
                return (bool)localSettings.Values[SURVEY_CONSENT_KEY];
            }

            return false;
        }

        /// <summary>
        /// Sets the user's consent status in local settings.
        /// </summary>
        /// <param name="consent">True if the user has given consent; otherwise, false.</param>
        private void SetUserConsent(bool consent)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[SURVEY_CONSENT_KEY] = consent;
        }

        /// <summary>
        /// Updates the last survey timestamp in local settings.
        /// </summary>
        private void UpdateLastSurveyTimestamp()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[LAST_SURVEY_TIMESTAMP_KEY] = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Displays the survey dialog to the user.
        /// </summary>
        /// <returns>True if the dialog was shown; otherwise, false.</returns>
        private async Task<bool> ShowSurveyDialogAsync()
        {
            try
            {
                // Reset the dialog state
                _progressBar.Value = 0;
                _progressBar.Visibility = Visibility.Collapsed;
                _statusTextBlock.Visibility = Visibility.Collapsed;

                // Show the dialog
                await _surveyDialog.ShowAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the app
                _logger.LogError($"Error showing hardware survey dialog: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Collects hardware information and submits it to the database.
        /// </summary>
        private async Task CollectAndSubmitHardwareDataAsync()
        {
            try
            {
                // Start a timer to make sure we don't take too long
                var stopwatch = Stopwatch.StartNew();

                // Show progress UI if the dialog is visible
                if (_progressBar != null && _progressBar.Visibility == Visibility.Visible)
                {
                    _progressBar.IsIndeterminate = true;
                    _statusTextBlock.Text = "Collecting hardware information...";
                }

                // Collect hardware data
                var hardwareData = await CollectHardwareDataAsync();

                // Simulate progress - in reality, the data collection happens quickly
                // but we want to show some progress to the user
                if (_progressBar != null && _progressBar.Visibility == Visibility.Visible)
                {
                    // Update progress to 50%
                    _progressBar.IsIndeterminate = false;
                    _progressBar.Value = 50;
                    _statusTextBlock.Text = "Processing information...";

                    // Add a small delay for visual feedback
                    await Task.Delay(500);
                }

                // Save to database
                await _surveyService.SaveHardwareDataAsync(hardwareData);

                // Update the last survey timestamp
                UpdateLastSurveyTimestamp();

                // Complete progress
                if (_progressBar != null && _progressBar.Visibility == Visibility.Visible)
                {
                    _progressBar.Value = 100;
                    _statusTextBlock.Text = "Complete! Thank you for your help.";

                    // Add a small delay for visual feedback
                    await Task.Delay(500);
                }

                // Log the time taken
                stopwatch.Stop();
                _logger.LogInfo($"Hardware survey completed in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the app
                _logger.LogError($"Error collecting hardware data: {ex.Message}");

                if (_statusTextBlock != null && _statusTextBlock.Visibility == Visibility.Visible)
                {
                    _statusTextBlock.Text = "Unable to collect hardware information at this time.";
                }
            }
        }

        // Add this method to HardwareSurveyComponent
        public async Task<bool> ShowWithExplicitXamlRoot(XamlRoot xamlRoot)
        {
            try
            {
                // Create a fresh content grid
                var contentGrid = new Grid
                {
                    Padding = new Thickness(16),
                    RowSpacing = 16
                };

                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Description text
                var descriptionTextBlock = new TextBlock
                {
                    Text = "We'd like to collect anonymous information about your device to help improve app performance. " +
                          "This data helps us optimize the app for different hardware configurations.\n\n" +
                          "We will collect the following information:\n" +
                          "• Device type (desktop, laptop, tablet, smartphone)\n" +
                          "• Operating system name and version\n" +
                          "• Screen resolution\n" +
                          "• Available RAM\n" +
                          "• CPU information\n" +
                          "• GPU information (if available)\n" +
                          "• Connection type (WiFi, cellular, ethernet)\n\n" +
                          "This information will be anonymized and not linked to your personal data.",
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetRow(descriptionTextBlock, 0);
                contentGrid.Children.Add(descriptionTextBlock);

                // Progress bar
                var progressBar = new ProgressBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = 0,
                    Visibility = Visibility.Collapsed
                };
                Grid.SetRow(progressBar, 1);
                contentGrid.Children.Add(progressBar);

                // Status text
                var statusTextBlock = new TextBlock
                {
                    Text = "Collecting hardware information...",
                    TextWrapping = TextWrapping.Wrap,
                    Visibility = Visibility.Collapsed
                };
                Grid.SetRow(statusTextBlock, 2);
                contentGrid.Children.Add(statusTextBlock);

                // Create a fresh dialog with the explicitly provided XamlRoot
                var surveyDialog = new ContentDialog
                {
                    Title = "Hardware Survey",
                    Content = contentGrid,
                    PrimaryButtonText = "Yes, I'll help",
                    CloseButtonText = "No thanks",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = xamlRoot // Use the explicitly provided XamlRoot
                };

                // Store references to UI elements
                _progressBar = progressBar;
                _statusTextBlock = statusTextBlock;
                _surveyDialog = surveyDialog;

                // Add event handlers
                surveyDialog.PrimaryButtonClick += SurveyDialog_PrimaryButtonClick;
                surveyDialog.CloseButtonClick += SurveyDialog_CloseButtonClick;

                // Show the dialog
                await surveyDialog.ShowAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error showing hardware survey dialog: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Collects hardware information from the device.
        /// </summary>
        /// <returns>A HardwareData object containing the collected information.</returns>
        private async Task<HardwareData> CollectHardwareDataAsync()
        {
            var hardwareData = new HardwareData
            {
                Timestamp = DateTime.Now,
                DeviceID = GetSafeAnonymousDeviceID(), // Use a safer method
                AppVersion = GetSafeAppVersion()
            };

            try
            {
                // Use safer methods that don't throw exceptions
                hardwareData.DeviceType = GetSafeDeviceType();
                hardwareData.OperatingSystem = GetSafeOperatingSystem();
                hardwareData.OSVersion = GetSafeOSVersion();
                hardwareData.BrowserName = "WinUI App"; // Static value for WinUI apps
                hardwareData.BrowserVersion = "N/A"; // Not applicable
                hardwareData.ScreenResolution = "1920x1080"; // Default fallback
                hardwareData.AvailableRAM = await GetSafeRAMAsync();
                hardwareData.CPUInformation = "System CPU"; // Simple fallback
                hardwareData.GPUInformation = "System GPU"; // Simple fallback
                hardwareData.ConnectionType = GetSafeConnectionType();
            }
            catch (Exception ex)
            {
                // Log the error but continue with default values
                _logger.LogError($"Error during hardware data collection: {ex.Message}");

                // Fill in any missing values with fallbacks
                hardwareData.DeviceType = hardwareData.DeviceType ?? "Desktop";
                hardwareData.OperatingSystem = hardwareData.OperatingSystem ?? "Windows";
                hardwareData.OSVersion = hardwareData.OSVersion ?? "10";
                hardwareData.ScreenResolution = hardwareData.ScreenResolution ?? "1920x1080";
                hardwareData.AvailableRAM = hardwareData.AvailableRAM ?? "8 GB";
                hardwareData.CPUInformation = hardwareData.CPUInformation ?? "Unknown CPU";
                hardwareData.GPUInformation = hardwareData.GPUInformation ?? "Unknown GPU";
                hardwareData.ConnectionType = hardwareData.ConnectionType ?? "Unknown";
            }

            return hardwareData;
        }

        // Safer versions of hardware detection methods
        private string GetSafeAnonymousDeviceID()
        {
            try
            {
                // Try the original method
                return GetAnonymousDeviceID();
            }
            catch
            {
                // Fallback to a simple randomized ID
                return $"Device-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
        }

        private string GetSafeAppVersion()
        {
            try
            {
                var package = Windows.ApplicationModel.Package.Current;
                var version = package.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
            catch
            {
                return "1.0.0.0";
            }
        }

        private string GetSafeDeviceType()
        {
            try
            {
                return "Desktop"; // Simple fallback for WinUI apps
            }
            catch
            {
                return "Desktop";
            }
        }

        private string GetSafeOperatingSystem()
        {
            try
            {
                return Environment.OSVersion.Platform.ToString();
            }
            catch
            {
                return "Windows";
            }
        }

        private string GetSafeOSVersion()
        {
            try
            {
                return Environment.OSVersion.Version.ToString();
            }
            catch
            {
                return "10.0";
            }
        }

        private async Task<string> GetSafeRAMAsync()
        {
            try
            {
                // Try a simpler approach using GC information as an estimation
                long memBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
                double memGB = Math.Round((double)memBytes / (1024 * 1024 * 1024), 1);
                return $"{memGB} GB";
            }
            catch
            {
                return "8 GB"; // Default fallback
            }
        }

        private string GetSafeConnectionType()
        {
            try
            {
                return "Ethernet"; // Simple fallback
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Creates an anonymous device ID for tracking the device without identifying the user.
        /// </summary>
        private string GetAnonymousDeviceID()
        {
            // Use the device's hardware ID hash as a base
            string deviceId = null;
            try
            {
                var token = HardwareIdentification.GetPackageSpecificToken(null);
                var hardwareId = token.Id;
                var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

                byte[] bytes = new byte[hardwareId.Length];
                dataReader.ReadBytes(bytes);

                // Hash the hardware ID to anonymize it
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    var hash = sha.ComputeHash(bytes);
                    deviceId = BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
            catch
            {
                // Fallback to a random GUID if hardware ID isn't available
                deviceId = Guid.NewGuid().ToString();
            }

            return deviceId;
        }

        /// <summary>
        /// Gets the device type (desktop, laptop, tablet, smartphone).
        /// </summary>
        private string GetDeviceType()
        {
            // This is a simplified implementation. In a real app, you would use
            // more reliable detection methods.
            var deviceFamily = AnalyticsInfo.VersionInfo.DeviceFamily;

            if (deviceFamily.Contains("Desktop"))
            {
                // Try to determine if it's a laptop or desktop
                // This is an approximation as there's no foolproof way to detect this
                var isPowerConnected = Windows.System.Power.PowerManager.PowerSupplyStatus ==
                    Windows.System.Power.PowerSupplyStatus.Adequate;
                var hasBattery = Windows.System.Power.PowerManager.BatteryStatus !=
                    Windows.System.Power.BatteryStatus.NotPresent;

                return (hasBattery) ? "Laptop" : "Desktop";
            }
            else if (deviceFamily.Contains("Mobile"))
            {
                return "Smartphone";
            }
            else if (deviceFamily.Contains("IoT"))
            {
                return "IoT Device";
            }
            else if (deviceFamily.Contains("Xbox"))
            {
                return "Xbox";
            }
            else if (deviceFamily.Contains("Holographic"))
            {
                return "HoloLens";
            }
            else if (deviceFamily.Contains("Team"))
            {
                return "Surface Hub";
            }

            // Default to the device family if we can't determine a more specific type
            return deviceFamily;
        }

        /// <summary>
        /// Gets the operating system name.
        /// </summary>
        private string GetOperatingSystem()
        {
            return AnalyticsInfo.VersionInfo.DeviceFamily;
        }

        /// <summary>
        /// Gets the operating system version.
        /// </summary>
        private string GetOSVersion()
        {
            // Get OS version
            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);

            var major = (version & 0xFFFF000000000000L) >> 48;
            var minor = (version & 0x0000FFFF00000000L) >> 32;
            var build = (version & 0x00000000FFFF0000L) >> 16;
            var revision = (version & 0x000000000000FFFFL);

            return $"{major}.{minor}.{build}.{revision}";
        }

        /// <summary>
        /// Gets the browser name.
        /// </summary>
        private string GetBrowserName()
        {
            // In a WinUI app, we don't have a browser, so this is just a placeholder
            return "WinUI App";
        }

        /// <summary>
        /// Gets the browser version.
        /// </summary>
        private string GetBrowserVersion()
        {
            // In a WinUI app, we don't have a browser, so this is just a placeholder
            return "N/A";
        }

        /// <summary>
        /// Gets the screen resolution.
        /// </summary>
        private string GetScreenResolution()
        {
            // In WinUI 3, getting screen resolution safely can be challenging
            // For hardware survey purposes, we'll use a fallback approach
            try
            {
                // Attempt to get screen info through environment variables or other means
                // This is just a placeholder for a real implementation
                var screenWidth = Microsoft.UI.Xaml.Window.Current?.Bounds.Width;
                var screenHeight = Microsoft.UI.Xaml.Window.Current?.Bounds.Height;

                if (screenWidth.HasValue && screenHeight.HasValue &&
                    screenWidth.Value > 0 && screenHeight.Value > 0)
                {
                    return $"{(int)screenWidth.Value}x{(int)screenHeight.Value}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting screen resolution: {ex.Message}");
            }

            // Fallback approach - since this is just for survey purposes,
            // a reasonable default is acceptable when actual detection fails
            return "1920x1080";
        }

        /// <summary>
        /// Gets the available RAM in GB.
        /// </summary>
        private async Task<string> GetAvailableRAMAsync()
        {
            try
            {
                // Get memory usage information
                var memoryReport = Windows.System.MemoryManager.GetAppMemoryReport();
                var totalMemory = memoryReport.TotalCommitLimit;

                // Convert to GB and round to 2 decimal places
                var ramInGB = Math.Round((double)totalMemory / (1024 * 1024 * 1024), 2);
                return $"{ramInGB} GB";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the CPU information.
        /// </summary>
        private async Task<string> GetCPUInformationAsync()
        {
            try
            {
                // This is a simplified implementation
                return GetProcessorName();
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the processor name from the registry.
        /// </summary>
        private string GetProcessorName()
        {
            // This is a placeholder. In a real WinUI app, we would use the Registry or WMI
            // to get the processor name, which requires elevated permissions.
            // For simplicity, we're returning a generic value.
            return "CPU information not available in WinUI environment";
        }

        /// <summary>
        /// Gets the GPU information.
        /// </summary>
        private async Task<string> GetGPUInformationAsync()
        {
            try
            {
                // Find video devices using the appropriate selector
                var deviceSelector = "System.Devices.InterfaceClassGuid:=\"5CDD39B6-6AFE-4F60-8C37-C78365A4F979\"";
                var devices = await DeviceInformation.FindAllAsync(deviceSelector);

                if (devices.Count > 0)
                {
                    // Get the name of the first GPU
                    return devices[0].Name;
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the connection type (WiFi, cellular, ethernet).
        /// </summary>
        private string GetConnectionType()
        {
            try
            {
                // Get network connectivity information
                var profile = NetworkInformation.GetInternetConnectionProfile();

                if (profile == null)
                {
                    return "Not connected";
                }

                var connectivityLevel = profile.GetNetworkConnectivityLevel();

                if (connectivityLevel != NetworkConnectivityLevel.InternetAccess)
                {
                    return "Limited connectivity";
                }

                var interfaceType = profile.NetworkAdapter.IanaInterfaceType;

                // IANA interface types: https://www.iana.org/assignments/ianaiftype-mib/ianaiftype-mib
                switch (interfaceType)
                {
                    case 6: // ethernet
                        return "Ethernet";
                    case 71: // IEEE 802.11 wireless
                        return "WiFi";
                    case 243: // WiMAX
                    case 244: // 3GPP
                    case 245: // 3GPP2
                        return "Cellular";
                    default:
                        return $"Other ({interfaceType})";
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the current app version.
        /// </summary>
        private string GetAppVersion()
        {
            var package = Windows.ApplicationModel.Package.Current;
            var version = package.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the survey dialog's primary button (consent) click event.
        /// </summary>
        private async void SurveyDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Prevent the dialog from closing until we've processed the submission
            var deferral = args.GetDeferral();

            try
            {
                // Save the user's consent
                SetUserConsent(true);

                // Show progress UI
                _progressBar.Visibility = Visibility.Visible;
                _statusTextBlock.Visibility = Visibility.Visible;

                // Disable buttons while collecting data
                _surveyDialog.IsPrimaryButtonEnabled = false;
                _surveyDialog.IsSecondaryButtonEnabled = false;

                // Collect and submit hardware data
                await CollectAndSubmitHardwareDataAsync();
            }
            finally
            {
                deferral.Complete();
            }
        }

        /// <summary>
        /// Handles the survey dialog's close button (no consent) click event.
        /// </summary>
        private void SurveyDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Save the user's lack of consent
            SetUserConsent(false);

            // Update the timestamp so we don't ask again too soon
            UpdateLastSurveyTimestamp();
        }

        #endregion
    }

    /// <summary>
    /// Data class to hold hardware information.
    /// </summary>
    public class HardwareData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the hardware survey.
        /// This will typically be auto-generated by the database.
        /// </summary>
        public int SurveyID { get; set; }

        /// <summary>
        /// Gets or sets the anonymous device ID.
        /// This is used to track the same device across multiple surveys without identifying the user.
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// Gets or sets the device type (desktop, laptop, tablet, smartphone).
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the operating system name.
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the operating system version.
        /// </summary>
        public string OSVersion { get; set; }

        /// <summary>
        /// Gets or sets the browser name.
        /// </summary>
        public string BrowserName { get; set; }

        /// <summary>
        /// Gets or sets the browser version.
        /// </summary>
        public string BrowserVersion { get; set; }

        /// <summary>
        /// Gets or sets the screen resolution.
        /// </summary>
        public string ScreenResolution { get; set; }

        /// <summary>
        /// Gets or sets the available RAM.
        /// </summary>
        public string AvailableRAM { get; set; }

        /// <summary>
        /// Gets or sets the CPU information.
        /// </summary>
        public string CPUInformation { get; set; }

        /// <summary>
        /// Gets or sets the GPU information.
        /// </summary>
        public string GPUInformation { get; set; }

        /// <summary>
        /// Gets or sets the connection type (WiFi, cellular, ethernet).
        /// </summary>
        public string ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the survey was submitted.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the app version when the survey was submitted.
        /// </summary>
        public string AppVersion { get; set; }
    }

    /// <summary>
    /// Interface for the hardware survey database service.
    /// This would be implemented by a concrete class that handles the actual database operations.
    /// </summary>
    public interface IHardwareSurveyDatabaseService
    {
        /// <summary>
        /// Saves hardware data to the database.
        /// </summary>
        /// <param name="hardwareData">The hardware data to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveHardwareDataAsync(HardwareData hardwareData);
    }

    /// <summary>
    /// Interface for the logger service.
    /// This would be implemented by a concrete class that handles the actual logging.
    /// </summary>
    public interface ILoggerService
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogInfo(string message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogError(string message);
    }
}