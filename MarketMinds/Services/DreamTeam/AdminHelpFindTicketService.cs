using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marketplace_SE.Data;
using Marketplace_SE.Objects;
using Marketplace_SE.Service;

namespace Marketplace_SE.Services.DreamTeam
{
    public class AdminFindHelpTicketService
    {
        public bool DoesUserIdExist(string userId)
        {
            return BackendUserGetHelp.DoesUserIDExist(userId);
        }

        public List<HelpTicket> GetHelpTicketsByUserId(string userId)
        {
            List<string> helpTicketIDs = BackendUserGetHelp.GetTicketIDsMatchingCriteria(userId);
            return BackendUserGetHelp.LoadTicketsFromDB(helpTicketIDs);
        }
    }
}

