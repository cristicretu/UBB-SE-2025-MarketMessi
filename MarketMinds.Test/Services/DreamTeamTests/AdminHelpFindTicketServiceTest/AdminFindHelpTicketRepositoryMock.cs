using System.Collections.Generic;
using System.Linq;
using MarketMinds.Models;

namespace MarketMinds.Test.Services.DreamTeamTests.AdminHelpFindTicketService
{
    public class AdminFindHelpTicketRepositoryMock
    {
        private List<HelpTicket> tickets;

        public AdminFindHelpTicketRepositoryMock()
        {
            tickets = new List<HelpTicket>();
        }

        public void AddHelpTicket(HelpTicket ticket)
        {
            tickets.Add(ticket);
        }

        public bool DoesUserIDExist(string userId)
        {
            return tickets.Any(t => t.UserID == userId);
        }

        public List<string> GetTicketIDsMatchingCriteria(string userId)
        {
            return tickets
                .Where(t => t.UserID == userId)
                .Select(t => t.TicketID)
                .ToList();
        }

        public List<HelpTicket> LoadTicketsFromDB(List<string> ticketIds)
        {
            return tickets
                .Where(t => ticketIds.Contains(t.TicketID))
                .ToList();
        }
    }
}
