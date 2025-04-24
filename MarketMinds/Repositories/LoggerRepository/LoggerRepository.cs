using System;
using DataAccessLayer;
using Microsoft.Data.SqlClient;

namespace Marketplace_SE.Repositories
{
    public class LoggerRepository : ILoggerRepository
    {
        private readonly DataBaseConnection connection;

        public LoggerRepository(DataBaseConnection dbConnection)
        {
            connection = dbConnection;
        }

        public void LogInfo(string message)
        {
            SaveLog("INFO", message);
        }

        public void LogError(string message)
        {
            SaveLog("ERROR", message);
        }

        private void SaveLog(string logLevel, string message)
        {
            try
            {
                string query = @"
                    INSERT INTO Logs (LogLevel, Message, Timestamp)
                    VALUES (@LogLevel, @Message, @Timestamp)";

                connection.OpenConnection();
                using (var command = new SqlCommand(query, connection.GetConnection()))
                {
                    command.Parameters.AddWithValue("@LogLevel", logLevel);
                    command.Parameters.AddWithValue("@Message", message);
                    command.Parameters.AddWithValue("@Timestamp", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving log to database: {ex.Message}");
            }
            finally
            {
                connection.CloseConnection();
            }
        }
    }
}
