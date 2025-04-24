using System.Collections.Generic;
using MarketMinds.Models;

namespace Marketplace_SE.Services
{
    // added this for new branch
    public interface IHelpTicketService
    {
        bool ValidateUser(string userID);
        int CreateHelpTicket(string userID, string userName, string description);
        List<HelpTicket> GetTicketsForUser(string userID);
        bool ValidateTicketID(int ticketID);
        int UpdateTicketDescription(string ticketID, string newDescription);
        int CloseTicket(string ticketID);
    }
}
