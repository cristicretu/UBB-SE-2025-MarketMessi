// RateAppComponent.cs
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace Marketplace_SE.Rating
{
    /// <summary>
    /// Implements the "Rate the App" feature that displays after a user completes a transaction.
    /// This component handles user ratings with a 5-star system and optional comments.
    /// </summary>
    public sealed partial class RateAppComponent : UserControl
    {
        #region Fields and Properties

        // Constants
        private const string LAST_RATING_TIMESTAMP_KEY = "LastRatingTimestamp";
        private const string LAST_RATING_SUBMISSION_KEY = "LastRatingSubmission";
        private const int RATING_REMINDER_INTERVAL_DAYS = 14; // Reappear after 14 days if "Remind Me Later" is clicked
        private const int RATING_DISABLE_INTERVAL_DAYS = 90;  // Disable for 90 days after submission


        // UI Elements
        private ContentDialog _ratingDialog;
        private RatingControl _ratingControl;
        private TextBox _commentTextBox;
        private Button _submitButton;
        private Button _remindLaterButton;
        private TextBlock _characterCountTextBlock;

        // State tracking
        private bool _hasCompletedTransaction = false;
        private double _selectedRating = 0;

        private bool _showThankYouMessage = false;

        // Database service dependency (would be injected in a production app)
        private readonly IRatingDatabaseService _ratingService;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Initializes a new instance of the RateAppComponent class.
        /// </summary>
        /// <param name="ratingService">Service for saving ratings to database</param>
        public RateAppComponent(IRatingDatabaseService ratingService)
        {
            _ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));
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
        /// Creates and initializes the rating dialog UI components.
        /// </summary>
        private void InitializeComponent()
        {
            // This method is now simpler as the actual UI is built in ShowRatingDialogAsync
            // We just initialize the fields to null to avoid any null reference issues
            _ratingControl = null;
            _commentTextBox = null;
            _characterCountTextBlock = null;
            _ratingDialog = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Call this method when a user completes a transaction (buy or sell).
        /// It marks that a transaction has been completed and the rating dialog should appear on next login.
        /// </summary>
        public void OnTransactionCompleted()
        {
            _hasCompletedTransaction = true;

            // In a real implementation, we would persist this state
            // For simplicity in this example, we're just using a field
        }

        /// <summary>
        /// Call this method when the user logs in to check if the rating dialog should be shown.
        /// </summary>
        /// <returns>True if the dialog was shown; otherwise, false.</returns>
        public async Task<bool> CheckAndShowRatingPromptAsync()
        {
            // If no completed transactions, don't show the dialog
            if (!_hasCompletedTransaction)
                return false;

            // Check if we should show the rating dialog based on time intervals
            if (ShouldShowRatingDialog())
            {
                return await ShowRatingDialogAsync();
            }

            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines if the rating dialog should be shown based on the last interaction time.
        /// </summary>
        /// <returns>True if the dialog should be shown; otherwise, false.</returns>
        private bool ShouldShowRatingDialog()
        {
            /*var localSettings = ApplicationData.Current.LocalSettings;

            // Check if user has submitted a rating recently
            if (localSettings.Values.ContainsKey(LAST_RATING_SUBMISSION_KEY))
            {
                var lastSubmissionTicks = (long)localSettings.Values[LAST_RATING_SUBMISSION_KEY];
                var lastSubmission = new DateTime(lastSubmissionTicks);

                // If the user submitted a rating less than RATING_DISABLE_INTERVAL_DAYS ago, don't show
                if ((DateTime.Now - lastSubmission).TotalDays < RATING_DISABLE_INTERVAL_DAYS)
                    return false;
            }

            // Check if "Remind Me Later" was clicked recently
            if (localSettings.Values.ContainsKey(LAST_RATING_TIMESTAMP_KEY))
            {
                var lastPromptTicks = (long)localSettings.Values[LAST_RATING_TIMESTAMP_KEY];
                var lastPrompt = new DateTime(lastPromptTicks);

                // If "Remind Me Later" was clicked less than RATING_REMINDER_INTERVAL_DAYS ago, don't show
                if ((DateTime.Now - lastPrompt).TotalDays < RATING_REMINDER_INTERVAL_DAYS)
                    return false;
            }*/

            return true;
        }

        /// <summary>
        /// Displays the rating dialog to the user.
        /// </summary>
        /// <returns>True if the dialog was shown; otherwise, false.</returns>

        private async Task<bool> ShowRatingDialogAsync()
        {
            try
            {
                // Create a fresh content grid each time
                var contentGrid = new Grid
                {
                    Padding = new Thickness(16),
                    RowSpacing = 16
                };
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Reset the rating control
                _ratingControl = new RatingControl
                {
                    Caption = "Please rate your experience:",
                    MaxRating = 5,
                    PlaceholderValue = 0,
                    IsReadOnly = false,
                    IsClearEnabled = false,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                _ratingControl.ValueChanged += RatingControl_ValueChanged;
                Grid.SetRow(_ratingControl, 0);
                contentGrid.Children.Add(_ratingControl);

                // Comment section
                var commentPanel = new StackPanel
                {
                    Spacing = 4
                };
                var commentLabel = new TextBlock
                {
                    Text = "Additional comments (optional):",
                    Margin = new Thickness(0, 4, 0, 4)
                };
                commentPanel.Children.Add(commentLabel);

                _commentTextBox = new TextBox
                {
                    PlaceholderText = "Tell us more about your experience...",
                    TextWrapping = TextWrapping.Wrap,
                    MaxLength = 500,
                    MinHeight = 100
                };
                _commentTextBox.TextChanged += CommentTextBox_TextChanged;
                commentPanel.Children.Add(_commentTextBox);

                // Character count
                _characterCountTextBlock = new TextBlock
                {
                    Text = "0/500 characters",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                commentPanel.Children.Add(_characterCountTextBlock);
                Grid.SetRow(commentPanel, 1);
                contentGrid.Children.Add(commentPanel);

                // Reset state
                _selectedRating = 0;
                _showThankYouMessage = false;

                // Create the dialog with the fresh content
                _ratingDialog = new ContentDialog
                {
                    Title = "App Rating",
                    Content = contentGrid,
                    PrimaryButtonText = "Submit",
                    SecondaryButtonText = "Remind Me Later",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                // Enable/disable submit button based on rating
                _ratingDialog.IsPrimaryButtonEnabled = false;
                _ratingDialog.PrimaryButtonClick += SubmitButton_Click;
                _ratingDialog.SecondaryButtonClick += RemindLaterButton_Click;

                // Show the dialog
                await _ratingDialog.ShowAsync();

                // Check if we need to show the thank you message
                if (_showThankYouMessage)
                {
                    _showThankYouMessage = false;
                    await ShowThankYouMessageAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the app
                // System.Diagnostics.Debug.WriteLine($"Error showing rating dialog: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves the rating to the database.
        /// </summary>
        /// <param name="rating">The user's rating (1-5).</param>
        /// <param name="comment">The user's optional comment.</param>
        /// <returns>True if the rating was saved successfully; otherwise, false.</returns>
        private async Task<bool> SaveRatingAsync(double rating, string comment)
        {
            try
            {
                // Create the rating data object
                var ratingData = new RatingData
                {
                    UserID = await GetCurrentUserIDAsync(),
                    Rating = rating,
                    Comment = comment,
                    Timestamp = DateTime.Now,
                    AppVersion = GetAppVersion()
                };

                // Save to database
                await _ratingService.SaveRatingAsync(ratingData);

                // Save the submission timestamp to prevent showing the dialog again too soon
                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values[LAST_RATING_SUBMISSION_KEY] = DateTime.Now.Ticks;

                // Reset the transaction completed flag
                _hasCompletedTransaction = false;

                return true;
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the app
                // System.Diagnostics.Debug.WriteLine($"Error saving rating: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the current user ID.
        /// In a real app, this would come from the authentication system.
        /// </summary>
        private async Task<string> GetCurrentUserIDAsync()
        {
            // This is just a placeholder.
            return "user-123";
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

        /// <summary>
        /// Shows a thank you message to the user after submitting a rating.
        /// </summary>
        private async Task ShowThankYouMessageAsync()
        {
            var thankYouDialog = new ContentDialog
            {
                Title = "Thank You!",
                Content = "Your feedback helps us improve the app for everyone.",
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            await thankYouDialog.ShowAsync();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the rating control's value changed event.
        /// </summary>
        private void RatingControl_ValueChanged(RatingControl sender, object args)
        {
            _selectedRating = sender.Value;

            // Enable the submit button only if a rating is selected (server-side validation will also occur)
            _ratingDialog.IsPrimaryButtonEnabled = _selectedRating > 0;
        }

        /// <summary>
        /// Handles the comment text box's text changed event to update the character count.
        /// </summary>
        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _characterCountTextBlock.Text = $"{_commentTextBox.Text.Length}/500 characters";
        }

        /// <summary>
        /// Handles the submit button click event.
        /// </summary>

        private void SubmitButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Prevent the dialog from closing until we've processed the submission
            var deferral = args.GetDeferral();
            try
            {
                // Validate the rating
                if (_selectedRating <= 0)
                {
                    // This shouldn't happen since we disable the submit button, but just in case
                    _ratingDialog.IsPrimaryButtonEnabled = false;
                    return;
                }

                // Save the rating
                var success = SaveRatingAsync(_selectedRating, _commentTextBox.Text).GetAwaiter().GetResult();

                if (success)
                {
                    // Set flag to show thank you message later, after this dialog closes
                    _showThankYouMessage = true;
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        /// <summary>
        /// Handles the "Remind Me Later" button click event.
        /// </summary>
        private void RemindLaterButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Save the current timestamp so we don't show the dialog again too soon
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[LAST_RATING_TIMESTAMP_KEY] = DateTime.Now.Ticks;
        }

        #endregion
    }

    /// <summary>
    /// Data class to hold rating information.
    /// </summary>
    public class RatingData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the rating.
        /// This will typically be auto-generated by the database.
        /// </summary>
        public int RatingID { get; set; }

        /// <summary>
        /// Gets or sets the user ID associated with the rating.
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Gets or sets the rating value (1-5 with precision to one decimal).
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Gets or sets the optional comment provided by the user.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the rating was submitted.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the app version when the rating was submitted.
        /// </summary>
        public string AppVersion { get; set; }
    }

    /// <summary>
    /// Interface for the rating database service.
    /// This would be implemented by a concrete class that handles the actual database operations.
    /// </summary>
    public interface IRatingDatabaseService
    {
        /// <summary>
        /// Saves a rating to the database.
        /// </summary>
        /// <param name="ratingData">The rating data to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveRatingAsync(RatingData ratingData);
    }
}