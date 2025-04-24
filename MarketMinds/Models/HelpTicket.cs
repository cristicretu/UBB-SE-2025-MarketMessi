using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMinds.Models
{
    // added this for new branch
    public class HelpTicket
    {
        public string TicketID { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string DateAndTime { get; set; }
        public string Description { get; set; }
        public string Closed { get; set; }

        public HelpTicket()
        {
        }

        public HelpTicket(string ticketID, string userID, string userName, string dateAndTime, string description, string closed)
        {
            TicketID = ticketID;
            UserID = userID;
            UserName = userName;
            DateAndTime = dateAndTime;
            Description = description;
            Closed = closed;
        }
    }
}
