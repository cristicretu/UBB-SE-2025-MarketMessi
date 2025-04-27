using System;
using System.Collections.Generic;
using MarketMinds.Models;
using Marketplace_SE.Repositories;

namespace MarketMinds.Test.Services.DreamTeamTests.HelpTicketService
{
    public class HelpTicketRepositoryMock : IHelpTicketRepository
    {
        private List<HelpTicket> tickets = new List<HelpTicket>();
        private HashSet<string> userIds = new HashSet<string>();
        private HashSet<int> ticketIds = new HashSet<int>();

        public bool DoesUserIDExist(string userID)
        {
            return userIds.Contains(userID);
        }

        public int AddHelpTicket(HelpTicket ticket)
        {
            if (ticket == null) return 0;
            tickets.Add(ticket);
            return 1; // Simulate 1 row affected
        }

        public List<HelpTicket> GetHelpTicketsByUserID(string userID)
        {
            return tickets.FindAll(t => t.UserID == userID);
        }

        public bool DoesTicketIDExist(int ticketID)
        {
            return ticketIds.Contains(ticketID);
        }

        public int UpdateHelpTicketDescription(string ticketID, string newDescription)
        {
            return 1; // Simulate success
        }

        public int CloseHelpTicket(string ticketID)
        {
            return 1; // Simulate success
        }

        // Helper methods for tests
        public void AddMockUserId(string userId)
        {
            userIds.Add(userId);
        }

        public void AddMockTicket(HelpTicket ticket)
        {
            tickets.Add(ticket);
        }

        public void AddMockTicketId(int ticketId)
        {
            ticketIds.Add(ticketId);
        }

        public List<HelpTicket> GetAddedTickets()
        {
            return tickets;
        }
    }
}
