using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarketMinds.Models;
using Marketplace_SE.Services;

namespace Marketplace_SE.ViewModels
{
    // added this for new branch
    public class HelpTicketViewModel : INotifyPropertyChanged
    {
        private readonly IHelpTicketService service;
        private const int SuccessStatusCode = 1;

        public ObservableCollection<HelpTicket> HelpTickets { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string StatusMessage { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public HelpTicketViewModel(IHelpTicketService service)
        {
            this.service = service;
            HelpTickets = new ObservableCollection<HelpTicket>();
        }

        public void LoadTickets()
        {
            if (!string.IsNullOrWhiteSpace(UserID))
            {
                var tickets = service.GetTicketsForUser(UserID);
                HelpTickets.Clear();
                foreach (var ticket in tickets)
                {
                    HelpTickets.Add(ticket);
                }
            }
        }

        public void CreateTicket()
        {
            if (string.IsNullOrWhiteSpace(UserID) || string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Description))
            {
                StatusMessage = "All fields are required.";
                OnPropertyChanged(nameof(StatusMessage));
                return;
            }

            var result = service.CreateHelpTicket(UserID, UserName, Description);
            if (result == SuccessStatusCode)
            {
                StatusMessage = "Ticket created successfully.";
                LoadTickets();
            }
            else
            {
                StatusMessage = "Failed to create ticket.";
            }

            OnPropertyChanged(nameof(StatusMessage));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool ValidateUser(string userId)
        {
           return service.ValidateUser(userId);
        }
    }
}
