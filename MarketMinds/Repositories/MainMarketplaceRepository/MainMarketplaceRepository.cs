using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;
using DomainLayer.Domain;
using Microsoft.Data.SqlClient;

namespace MarketMinds.Repositories.MainMarketplaceRepository
{
    public class MainMarketplaceRepository : IMainMarketplaceRepository
    {
        private readonly DataBaseConnection dataBaseConnection;
        public MainMarketplaceRepository(DataBaseConnection dataBaseConnection)
        {
            this.dataBaseConnection = dataBaseConnection;
        }
        public List<UserNotSoldOrder> GetAvailableItems()
        {
            List<UserNotSoldOrder> availableItems = new List<UserNotSoldOrder>();
            var getAvailableItemsQuery = @"
            SELECT * 
            FROM Orders
            WHERE buyerId = -1";

            try
            {
                dataBaseConnection.OpenConnection();

                using (SqlCommand getAvailableItemsCommand = new SqlCommand(getAvailableItemsQuery, dataBaseConnection.GetConnection()))
                {
                    using (SqlDataReader reader = getAvailableItemsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            availableItems.Add(MapReaderToUserNotSoldOrder(reader));
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dataBaseConnection.CloseConnection();
            }
            return availableItems;
        }

        private UserNotSoldOrder MapReaderToUserNotSoldOrder(SqlDataReader reader)
        {
            return new UserNotSoldOrder
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Cost = (float)reader.GetDouble(reader.GetOrdinal("cost")),
                SellerId = reader.GetInt32(reader.GetOrdinal("sellerId")),
                BuyerId = reader.GetInt32(reader.GetOrdinal("buyerId")) // Will be -1 based on query
            };
        }
    }
}
