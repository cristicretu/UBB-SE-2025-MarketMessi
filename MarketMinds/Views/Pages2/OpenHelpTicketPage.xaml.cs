using Microsoft.UI.Xaml.Controls;
using Marketplace_SE.ViewModels;
using MarketMinds.Repositories.HelpTicketRepository;
using Marketplace_SE.Data;
using Marketplace_SE.Services;
using DataAccessLayer;
using MarketMinds;

namespace Marketplace_SE.Views.Pages2
{
    public sealed partial class OpenHelpTicketPage : Page
    {
        public HelpTicketViewModel ViewModel { get; }

        public OpenHelpTicketPage()
        {
            this.InitializeComponent();

            DataBaseConnection connection = App.DatabaseConnection;
            var repository = new HelpTicketRepository(connection);
            var service = new HelpTicketService(repository);

            ViewModel = new HelpTicketViewModel(service);
            this.DataContext = ViewModel;
        }
    }
}
