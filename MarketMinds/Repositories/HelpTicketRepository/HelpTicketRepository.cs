using System;
using System.Collections.Generic;
using System.Data;
using MarketMinds.Models;
using DataAccessLayer;
using Microsoft.Data.SqlClient;
using Marketplace_SE.Repositories;

namespace MarketMinds.Repositories.HelpTicketRepository
{
    // added this for new branch
    public class HelpTicketRepository : IHelpTicketRepository
    {
        private readonly DataBaseConnection connection;

        public HelpTicketRepository(DataBaseConnection connection)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public bool DoesUserIDExist(string userID)
        {
            try
            {
                connection.OpenConnection();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.UserGetHelpTickets WHERE UserID=@UID", connection.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UID", userID);
                    var result = (int)command.ExecuteScalar();
                    return result > 0;
                }
            }
            finally
            {
                connection.CloseConnection();
            }
        }

        public int AddHelpTicket(HelpTicket ticket)
        {
            try
            {
                connection.OpenConnection();
                using (var command = new SqlCommand("INSERT INTO dbo.UserGetHelpTickets (UserID, UserName, DateAndTime, Descript, Closed) VALUES (@UID, @UN, @DAT, @D, @C)", connection.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UID", ticket.UserID);
                    command.Parameters.AddWithValue("@UN", ticket.UserName);
                    command.Parameters.AddWithValue("@DAT", ticket.DateAndTime);
                    command.Parameters.AddWithValue("@D", ticket.Description);
                    command.Parameters.AddWithValue("@C", ticket.Closed);

                    return command.ExecuteNonQuery(); // Returns the number of rows affected
                }
            }
            finally
            {
                connection.CloseConnection();
            }
        }

        public List<HelpTicket> GetHelpTicketsByUserID(string userID)
        {
            var tickets = new List<HelpTicket>();

            try
            {
                connection.OpenConnection();
                using (var command = new SqlCommand("SELECT * FROM dbo.UserGetHelpTickets WHERE UserID=@UID", connection.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UID", userID);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tickets.Add(new HelpTicket
                            {
                                TicketID = reader["TicketID"].ToString(),
                                UserID = reader["UserID"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                DateAndTime = reader["DateAndTime"].ToString(),
                                Description = reader["Descript"].ToString(),
                                Closed = reader["Closed"].ToString()
                            });
                        }
                    }
                }
            }
            finally
            {
                connection.CloseConnection();
            }

            return tickets;
        }

        public bool DoesTicketIDExist(int ticketID)
        {
            try
            {
                connection.OpenConnection();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.UserGetHelpTickets WHERE TicketID=@TID", connection.GetConnection()))
                {
                    command.Parameters.AddWithValue("@TID", ticketID);
                    var result = (int)command.ExecuteScalar();
                    return result > 0;
                }
            }
            finally
            {
                connection.CloseConnection();
            }
        }

        public int UpdateHelpTicketDescription(string ticketID, string newDescription)
        {
            try
            {
                connection.OpenConnection();
                using (var command = new SqlCommand("UPDATE dbo.UserGetHelpTickets SET Descript=@D WHERE TicketID=@TID", connection.GetConnection()))
                {
                    command.Parameters.AddWithValue("@TID", ticketID);
                    command.Parameters.AddWithValue("@D", newDescription);

                    return command.ExecuteNonQuery(); // Returns the number of rows affected
                }
            }
            finally
            {
                connection.CloseConnection();
            }
        }

        public int CloseHelpTicket(string ticketID)
        {
            try
            {
                connection.OpenConnection();
                using (var command = new SqlCommand("UPDATE dbo.UserGetHelpTickets SET Closed=@C WHERE TicketID=@TID", connection.GetConnection()))
                {
                    command.Parameters.AddWithValue("@TID", ticketID);
                    command.Parameters.AddWithValue("@C", "Yes");

                    return command.ExecuteNonQuery(); // Returns the number of rows affected
                }
            }
            finally
            {
                connection.CloseConnection();
            }
        }
    }
}
