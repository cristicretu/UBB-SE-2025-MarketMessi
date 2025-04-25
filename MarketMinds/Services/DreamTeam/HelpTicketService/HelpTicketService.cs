using System;
using System.Collections.Generic;
using MarketMinds.Models;
using MarketMinds.Repositories.HelpTicketRepository;
using Marketplace_SE.Repositories;

namespace Marketplace_SE.Services
{
    // added this for new branch
    public class HelpTicketService : IHelpTicketService
    {
        private readonly IHelpTicketRepository repository;

        public HelpTicketService(IHelpTicketRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public bool ValidateUser(string userID)
        {
            return repository.DoesUserIDExist(userID);
        }

        public int CreateHelpTicket(string userID, string userName, string description)
        {
            var ticket = new HelpTicket
            {
                UserID = userID,
                UserName = userName,
                DateAndTime = DateTime.Now.ToString("dd-MM-yyyy-HH-mm"),
                Description = description,
                Closed = "No"
            };

            return repository.AddHelpTicket(ticket);
        }

        public List<HelpTicket> GetTicketsForUser(string userID)
        {
            return repository.GetHelpTicketsByUserID(userID);
        }

        public bool ValidateTicketID(int ticketID)
        {
            return repository.DoesTicketIDExist(ticketID);
        }

        public int UpdateTicketDescription(string ticketID, string newDescription)
        {
            return repository.UpdateHelpTicketDescription(ticketID, newDescription);
        }

        public int CloseTicket(string ticketID)
        {
            return repository.CloseHelpTicket(ticketID);
        }
    }
}
