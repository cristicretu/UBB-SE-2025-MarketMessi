using System.Collections.Generic;
using MarketMinds.Models;

namespace Marketplace_SE.Repositories
{
    // added this for new branch
    public interface IHelpTicketRepository
    {
        bool DoesUserIDExist(string userID);
        int AddHelpTicket(HelpTicket ticket);
        List<HelpTicket> GetHelpTicketsByUserID(string userID);
        bool DoesTicketIDExist(int ticketID);
        int UpdateHelpTicketDescription(string ticketID, string newDescription);

        int CloseHelpTicket(string ticketID);
    }
}
