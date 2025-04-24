using System;
using Marketplace_SE.Data;
using System.Collections.Generic;
using Marketplace_SE.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Windows.Media.ClosedCaptioning;

namespace Marketplace_SE.Service
{
    static class BackendUserGetHelp
    {
        public enum BackendUserGetHelpStatusCodes : int
        {
            PushNewHelpTicketToDBSuccess,
            PushNewHelpTicketToDBFailure,
            UpdateHelpTicketInDBSuccess,
            UpdateHelpTicketInDBFailure,
            ClosedHelpTicketInDBSuccess,
            ClosedHelpTicketInDBFailure
        }
        public static int PushNewHelpTicketToDB(string userID, string userName, string description, string closed)
        {

            Database.Databasee = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            bool status = Database.Databasee.Connect();

            if (!status)
            {
                //database connection failed
                //ShowDialog("Database connection error", "Error connecting to database");

                Notification notification = new Notification("Database connection error", "Error connecting to database");
                notification.OkButton.Click += (s, e) =>
                {
                    notification.GetWindow().Close();
                    Database.Databasee.Close();
                };
                notification.GetWindow().Activate();
                return (int)BackendUserGetHelpStatusCodes.PushNewHelpTicketToDBFailure;
            }

            Database.Databasee.Execute("INSERT INTO dbo.UserGetHelpTickets (UserID, UserName, DateAndTime, Descript, Closed) VALUES (@UID, @UN, @DAT, @D, @C)",
                    new string[]
                    {
                        "@UID",
                        "@UN",
                        "@DAT",
                        "@D",
                        "@C"
                    }, new object[]
                    {
                        userID,
                        userName,
                        DateTime.Now.ToString("dd-MM-yyyy-HH-mm"),
                        Description,
                        closed
                    }
                );

            Database.Databasee.Close();

            return (int)BackendUserGetHelpStatusCodes.PushNewHelpTicketToDBSuccess;
        }

        public static bool DoesUserIDExist(string UserID)
        {
            return true;
        }

        public static List<HelpTicket> LoadTicketsFromDB(List<string> TicketIDs)
        {
            List<HelpTicket> returnList = new List<HelpTicket>();

            Database.Databasee = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            bool status = Database.Databasee.Connect();

            if (!status)
            {
                //database connection failed
                
                //ShowDialog("Database connection error", "Error connecting to database");

                Notification notification = new Notification("Database connection error", "Error connecting to database");
                notification.OkButton.Click += (s, e) =>
                {
                    notification.GetWindow().Close();
                    Database.Databasee.Close();
                };
                notification.GetWindow().Activate();
                return returnList;
            }

            List<HelpTicketFromDB> ticketsList = new List<HelpTicketFromDB>();

            foreach (string each in TicketIDs)
            {
                var data = Database.Databasee.Get("SELECT * FROM dbo.UserGetHelpTickets WHERE (TicketID=@TID)",
                    new string[]
                    {
                    "@TID"
                    }, new object[]
                    {
                    int.Parse(each)
                    });

                List<HelpTicketFromDB> temp = Database.Databasee.ConvertToObject<HelpTicketFromDB>(data);
                ticketsList.Add(temp[0]);
            }

            foreach(HelpTicketFromDB each in ticketsList)
            {
                returnList.Add(HelpTicket.FromHelpTicketFromDB(each));
            }

            Database.Databasee.Close();

            return returnList;
        }

        public static List<string> GetTicketIDsMatchingCriteria(string UserID)
        {
            List<string> returnList = new List<string>();

            Database.Databasee = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            bool status = Database.Databasee.Connect();

            if (!status)
            {
                //database connection failed
                //ShowDialog("Database connection error", "Error connecting to database");

                Notification notification = new Notification("Database connection error", "Error connecting to database");
                notification.OkButton.Click += (s, e) =>
                {
                    notification.GetWindow().Close();
                    Database.Databasee.Close();
                };
                notification.GetWindow().Activate();
                return new List<string>();
            }

            var data = Database.Databasee.Get("SELECT * FROM dbo.UserGetHelpTickets WHERE (UserID=@UID)",
                new string[]
                {
                    "@UID"
                }, new object[]
                {
                    UserID
                });

            List<HelpTicketFromDB> ticketsList = Database.Databasee.ConvertToObject<HelpTicketFromDB>(data);

            Database.Databasee.Close();

            foreach (HelpTicketFromDB each in ticketsList)
            {
                returnList.Add(each.TicketID.ToString());
            }

            return returnList;
        }

        public static bool DoesTicketIDExist(int RequestedTicketID)
        {
            Database.Databasee = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            bool status = Database.Databasee.Connect();

            if (!status)
            {
                Notification notification = new Notification("Database connection error", "Error connecting to database");
                notification.OkButton.Click += (s, e) =>
                {
                    notification.GetWindow().Close();
                    Database.Databasee.Close();
                };
                notification.GetWindow().Activate();
                return false;
            }

            var data = Database.Databasee.Get("SELECT * FROM dbo.UserGetHelpTickets WHERE (TicketID=@TID)",
                new string[]
                {
                    "@TID"
                }, new object[]
                {
                    RequestedTicketID
                });

            List<HelpTicketFromDB> ticketsList = Database.Databasee.ConvertToObject<HelpTicketFromDB>(data);

            Database.Databasee.Close();
            if (ticketsList.Count > 0)
            {
                return true;
            }

            return false;
        }

        public static int UpdateHelpTicketDescriptionInDB(string TicketID, string NewDescription)
        {
            Database.Databasee = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            bool status = Database.Databasee.Connect();

            if (!status)
            {
                //database connection failed
                //ShowDialog("Database connection error", "Error connecting to database");
                Notification notification = new Notification("Database connection error", "Error connecting to database");
                notification.OkButton.Click += (s, e) =>
                {
                    notification.GetWindow().Close();
                    Database.Databasee.Close();
                };
                notification.GetWindow().Activate();
                return (int)BackendUserGetHelpStatusCodes.UpdateHelpTicketInDBFailure;
            }

            Database.Databasee.Execute("UPDATE dbo.UserGetHelpTickets SET Descript=@D WHERE TicketID=@TID",
                    new string[]
                    {
                        "@TID",
                        "@D"
                    }, new object[]
                    {
                        int.Parse(TicketID),
                        NewDescription
                    }
                );

            object value = Database.Databasee.Close();

            return (int)BackendUserGetHelpStatusCodes.UpdateHelpTicketInDBSuccess;
        }

        public static int CloseHelpTicketInDB(string TicketID)
        {
            Database.Databasee = new Database(@"Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-45FVE4D\SQLEXPRESS;initial catalog=Marketplace_SE_UserGetHelp;trusted_connection=true");
            bool status = Database.Databasee.Connect();

            if (!status)
            {
                //database connection failed
                //ShowDialog("Database connection error", "Error connecting to database");

                Notification notification = new Notification("Database connection error", "Error connecting to database");
                notification.OkButton.Click += (s, e) =>
                {
                    notification.GetWindow().Close();
                    Database.Databasee.Close();
                };
                notification.GetWindow().Activate();
                return (int)BackendUserGetHelpStatusCodes.ClosedHelpTicketInDBFailure;
            }

            Database.Databasee.Execute("UPDATE dbo.UserGetHelpTickets SET Closed=@C WHERE TicketID=@TID",
                    new string[]
                    {
                        "@TID",
                        "@C"
                    }, new object[]
                    {
                        int.Parse(TicketID),
                        "Yes"
                    }
                );

            object value = Database.Databasee.Close();

            return (int)BackendUserGetHelpStatusCodes.ClosedHelpTicketInDBSuccess;
        }
    }

    public class HelpTicket
    {
        public string TicketID { get; }
        public string UserID { get; }
        public string UserName { get; }
        public string DateAndTime { get; }
        public string Descript { get; }
        public string Closed { get; }

        public HelpTicket(string ticketID_, string userID_, string userName_, string dateHour_, string description_, string Closed_)
        {
            TicketID = TicketID_;
            UserID = UserID_;
            UserName = UserName_;
            DateAndTime = DateHour_;
            Descript = Description_;
            Closed = closed_;
        }

        public string toStringExceptDescription()
        {
            return TicketID + "::" + UserID + "::" + UserName + "::" + DateAndTime + "::" + Closed;
        }

        public static HelpTicket FromHelpTicketFromDB(HelpTicketFromDB other)
        {
            return new HelpTicket(other.TicketID.ToString(), other.UserID, other.UserName, other.DateAndTime, other.Descript, other.Closed);
        }
    }

    public class HelpTicketFromDB
    {
        public int TicketID;
        public string UserID;
        public string UserName;
        public string DateAndTime;
        public string Descript;
        public string Closed;
    }
}
