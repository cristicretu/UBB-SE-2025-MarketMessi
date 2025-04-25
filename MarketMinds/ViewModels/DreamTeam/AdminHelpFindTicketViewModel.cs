using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marketplace_SE.Objects;
using Marketplace_SE.Service;
using Marketplace_SE.Services.DreamTeam;

namespace Marketplace_SE
{
    public class AdminFindHelpTicketViewModel
    {
        public List<HelpTicket> HelpTickets { get; private set; }
        public bool IsUserIdInvalid { get; private set; }
        public bool IsUserIdNotFound { get; private set; }
        public bool HasErrors => IsUserIdInvalid || IsUserIdNotFound;

        private readonly AdminFindHelpTicketService service;

        public AdminFindHelpTicketViewModel()
        {
            service = new AdminFindHelpTicketService();
            HelpTickets = new List<HelpTicket>();
        }

        public void SearchHelpTickets(string userId)
        {
            // Reset error states
            IsUserIdInvalid = false;
            IsUserIdNotFound = false;

            if (string.IsNullOrWhiteSpace(userId))
            {
                IsUserIdInvalid = true;
                return;
            }

            if (!service.DoesUserIdExist(userId))
            {
                IsUserIdNotFound = true;
                return;
            }

            HelpTickets = service.GetHelpTicketsByUserId(userId);
        }
    }
}

