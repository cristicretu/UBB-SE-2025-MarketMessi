using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marketplace_SE.Services.DreamTeam;
using Marketplace_SE.Objects;

namespace Marketplace_SE.ViewModel.DreamTeam
{
    public class AccountPageViewModel
    {
        public User Me { get; private set; }
        public List<UserOrder> Orders { get; private set; }
        public UserOrder SelectedOrder { get; set; }

        private readonly AccountPageService service;

        public AccountPageViewModel()
        {
            service = new AccountPageService();
        }

        public void Initialize()
        {
            Me = service.GetCurrentUser();
            Orders = service.GetUserOrders(Me.Id);
        }
    }
}

