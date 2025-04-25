using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DomainLayer.Domain;
using Marketplace_SE.Data;

namespace Marketplace_SE.Services.DreamTeam
{
    public class AccountPageService
    {
        public User GetCurrentUser()
        {
            var user = new User(0, "test", string.Empty);
            return user;
        }

        public List<UserOrder> GetUserOrders(int userId)
        {
            Database.Databases = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            bool status = Database.Databases.Connect();

            if (!status)
            {
                throw new System.Exception("Database connection error");
            }

            var data = Database.Databases.Get("SELECT * FROM Orders WHERE sellerId=@MyId OR buyerId=@MyId", new string[]
            {
                "@MyId"
            }, new object[]
            {
                userId
            });

            List<UserOrder> orders = Database.Databases.ConvertToObject<UserOrder>(data);

            // Sort by creation time descending
            orders.Sort((a, b) => (int)(b.Created - a.Created));

            return orders;
        }
    }
}
