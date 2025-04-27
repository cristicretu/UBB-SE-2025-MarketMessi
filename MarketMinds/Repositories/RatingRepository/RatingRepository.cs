using System;
using Marketplace_SE.Rating;
using DataAccessLayer;
using Microsoft.Data.SqlClient;

namespace Marketplace_SE.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly DataBaseConnection connection;

        public RatingRepository(DataBaseConnection dbConnection)
        {
            connection = dbConnection;
        }

        public void SaveRating(RatingData ratingData)
        {
            try
            {
                string query = @"
                    INSERT INTO Ratings (UserID, Rating, Comment, Timestamp, AppVersion)
                    VALUES (@UserID, @Rating, @Comment, @Timestamp, @AppVersion)";

                string[] arguments = { "@UserID", "@Rating", "@Comment", "@Timestamp", "@AppVersion" };
                object[] values = { ratingData.UserID, ratingData.Rating, ratingData.Comment ?? (object)DBNull.Value, ratingData.Timestamp, ratingData.AppVersion };

                connection.OpenConnection();
                using (var command = new SqlCommand(query, connection.GetConnection()))
                {
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        command.Parameters.AddWithValue(arguments[i], values[i]);
                    }
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving rating to database: {exception.Message}");
                throw;
            }
            finally
            {
                connection.CloseConnection();
            }
        }
    }
}
