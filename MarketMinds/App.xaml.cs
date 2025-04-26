using System;
using System.IO;
using Microsoft.UI.Xaml;
using BusinessLogicLayer.ViewModel;
using DataAccessLayer;
using DomainLayer.Domain;
using ViewModelLayer.ViewModel;
using Microsoft.Extensions.Configuration;
using MarketMinds.Services.AuctionProductsService;
using MarketMinds.Services.BorrowProductsService;
using MarketMinds.Services.BasketService;
using MarketMinds.Services.BuyProductsService;
using MarketMinds.Services.ProductCategoryService;
using MarketMinds.Services.ProductConditionService;
using MarketMinds.Services.ReviewService;
using MarketMinds.Services.ProductTagService;
using MarketMinds.Services;
using MarketMinds.ViewModels;
using MarketMinds.Services.DreamTeam.ChatBotService;
using MarketMinds.Repositories.ChatBotRepository;
using MarketMinds.Repositories.ChatRepository;
using MarketMinds.Services.DreamTeam.ChatService;
using MarketMinds.Repositories.MainMarketplaceRepository;
using MarketMinds.Services.DreamTeam.MainMarketplaceService;
using MarketMinds.Services.ImagineUploadService;
using MarketMinds.Services.UserService;

namespace MarketMinds
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration Configuration;
        public static DataBaseConnection DatabaseConnection;
        // Repository declarations
        public static ChatBotRepository ChatBotRepository;
        public static ChatRepository ChatRepository;
        public static MainMarketplaceRepository MainMarketplaceRepository;

        // Service declarations
        public static ProductService ProductService;
        public static BuyProductsService BuyProductsService;
        public static BorrowProductsService BorrowProductsService;
        public static AuctionProductsService AuctionProductsService;
        public static ProductCategoryService CategoryService;
        public static ProductTagService TagService;
        public static ProductConditionService ConditionService;
        public static ReviewsService ReviewsService;
        public static BasketService BasketService;
        public static ChatBotService ChatBotService;
        public static ChatService ChatService;
        public static MainMarketplaceService MainMarketplaceService;
        public static IImageUploadService ImageUploadService;
        public static IUserService UserService;

        // ViewModel declarations
        public static BuyProductsViewModel BuyProductsViewModel { get; private set; }
        public static BorrowProductsViewModel BorrowProductsViewModel { get; private set; }
        public static AuctionProductsViewModel AuctionProductsViewModel { get; private set; }
        public static ProductCategoryViewModel ProductCategoryViewModel { get; private set; }
        public static ProductConditionViewModel ProductConditionViewModel { get; private set; }
        public static ProductTagViewModel ProductTagViewModel { get; private set; }
        public static SortAndFilterViewModel AuctionProductSortAndFilterViewModel { get; private set; }
        public static SortAndFilterViewModel BorrowProductSortAndFilterViewModel { get; private set; }
        public static SortAndFilterViewModel BuyProductSortAndFilterViewModel { get; private set; }
        public static ReviewCreateViewModel ReviewCreateViewModel { get; private set; }
        public static SeeBuyerReviewsViewModel SeeBuyerReviewsViewModel { get; private set; }
        public static SeeSellerReviewsViewModel SeeSellerReviewsViewModel { get; private set; }
        public static BasketViewModel BasketViewModel { get; private set; }
        public static CompareProductsViewModel CompareProductsViewModel { get; private set; }
        public static ChatBotViewModel ChatBotViewModel { get; private set; }
        public static ChatViewModel ChatViewModel { get; private set; }
        public static MainMarketplaceViewModel MainMarketplaceViewModel { get; private set; }
        public static LoginViewModel LoginViewModel { get; private set; }
        public static RegisterViewModel RegisterViewModel { get; private set; }
        public static User CurrentUser { get; set; }
        public static User TestingUser { get; set; }

        private const int BUYER = 1;
        private const int SELLER = 2;

        private static IConfiguration appConfiguration;
        public static Window loginWindow = null!;
        public static Window mainWindow = null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            // Initialize configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            appConfiguration = builder.Build();
            // Initialize API configuration
            InitializeConfiguration();
        }

        private IConfiguration InitializeConfiguration()
        {
            Configuration = appConfiguration;
            return Configuration;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Create but don't show the main window yet
            mainWindow = new UiLayer.MainWindow();
            
            // Instantiate database connection with configuration
            DatabaseConnection = new DataBaseConnection(Configuration);
            
            // Instantiate repositories
            ChatBotRepository = new ChatBotRepository(DatabaseConnection);
            ChatRepository = new ChatRepository(DatabaseConnection);
            MainMarketplaceRepository = new MainMarketplaceRepository(DatabaseConnection);

            // Instantiate services
            BuyProductsService = new BuyProductsService(Configuration);
            BorrowProductsService = new BorrowProductsService(Configuration);
            AuctionProductsService = new AuctionProductsService(Configuration);
            CategoryService = new ProductCategoryService(Configuration);
            TagService = new ProductTagService(Configuration);
            ConditionService = new ProductConditionService(Configuration);
            ReviewsService = new ReviewsService(Configuration);
            BasketService = new BasketService(Configuration);
            UserService = new UserService(Configuration);
            ChatBotService = new ChatBotService(ChatBotRepository);
            ChatService = new ChatService(ChatRepository);
            MainMarketplaceService = new MainMarketplaceService(MainMarketplaceRepository);
                        
            // Initialize ImageUploadService if necessary
            if (ImageUploadService == null)
            {
                // Implement or use a mock service as appropriate
                // ImageUploadService = new LocalImageUploadService();
            }

            // Instantiate view models
            BuyProductsViewModel = new BuyProductsViewModel(BuyProductsService);
            AuctionProductsViewModel = new AuctionProductsViewModel(AuctionProductsService);
            ProductCategoryViewModel = new ProductCategoryViewModel(CategoryService);
            ProductTagViewModel = new ProductTagViewModel(TagService);
            ProductConditionViewModel = new ProductConditionViewModel(ConditionService);
            BorrowProductsViewModel = new BorrowProductsViewModel(BorrowProductsService);
            AuctionProductSortAndFilterViewModel = new SortAndFilterViewModel(AuctionProductsService);
            BorrowProductSortAndFilterViewModel = new SortAndFilterViewModel(BorrowProductsService);
            BuyProductSortAndFilterViewModel = new SortAndFilterViewModel(BuyProductsService);
            ReviewCreateViewModel = new ReviewCreateViewModel(ReviewsService, CurrentUser, TestingUser);
            SeeSellerReviewsViewModel = new SeeSellerReviewsViewModel(ReviewsService, TestingUser, TestingUser);
            SeeBuyerReviewsViewModel = new SeeBuyerReviewsViewModel(ReviewsService, CurrentUser);
            BasketViewModel = new BasketViewModel(CurrentUser, BasketService);
            CompareProductsViewModel = new CompareProductsViewModel();
            ChatBotViewModel = new ChatBotViewModel(ChatBotService);
            ChatViewModel = new ChatViewModel(ChatService);
            MainMarketplaceViewModel = new MainMarketplaceViewModel(MainMarketplaceService);
            LoginViewModel = new LoginViewModel(UserService);
            RegisterViewModel = new RegisterViewModel(UserService);
            
            // Show login window first instead of main window
            loginWindow = new LoginWindow();
            loginWindow.Activate();
        }
        
        // Method to be called after successful login
        public static void ShowMainWindow()
        {
            if (CurrentUser != null)
            {
                // Close login window
                if (loginWindow != null)
                {
                    loginWindow.Close();
                }
                
                // Show main window
                mainWindow.Activate();
            }
        }
    }
}
