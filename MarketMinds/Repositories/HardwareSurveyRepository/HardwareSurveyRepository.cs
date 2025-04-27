using System;
using Marketplace_SE.HardwareSurvey;
using DataAccessLayer;
using Microsoft.Data.SqlClient;

namespace Marketplace_SE.Repositories
{
    public class HardwareSurveyRepository : IHardwareSurveyRepository
    {
        private readonly DataBaseConnection connection;

        public HardwareSurveyRepository(DataBaseConnection databaseConnection)
        {
            connection = databaseConnection;
        }

        public void SaveHardwareData(HardwareData hardwareData)
        {
            try
            {
                string query = @"
                    INSERT INTO HardwareSurvey (
                        DeviceID, DeviceType, OperatingSystem, OSVersion, 
                        BrowserName, BrowserVersion, ScreenResolution, 
                        AvailableRAM, CPUInformation, GPUInformation, 
                        ConnectionType, Timestamp, AppVersion
                    )
                    VALUES (
                        @DeviceID, @DeviceType, @OperatingSystem, @OSVersion,
                        @BrowserName, @BrowserVersion, @ScreenResolution,
                        @AvailableRAM, @CPUInformation, @GPUInformation,
                        @ConnectionType, @Timestamp, @AppVersion
                    )";

                string[] arguments =
                {
                    "@DeviceID", "@DeviceType", "@OperatingSystem", "@OSVersion",
                    "@BrowserName", "@BrowserVersion", "@ScreenResolution",
                    "@AvailableRAM", "@CPUInformation", "@GPUInformation",
                    "@ConnectionType", "@Timestamp", "@AppVersion"
                };

                object[] values =
                {
                    hardwareData.DeviceID,
                    hardwareData.DeviceType,
                    hardwareData.OperatingSystem,
                    hardwareData.OperatingSystemVersion,
                    hardwareData.BrowserName ?? (object)DBNull.Value,
                    hardwareData.BrowserVersion ?? (object)DBNull.Value,
                    hardwareData.ScreenResolution,
                    hardwareData.AvailableRAM,
                    hardwareData.CPUInformation ?? (object)DBNull.Value,
                    hardwareData.GPUInformation ?? (object)DBNull.Value,
                    hardwareData.ConnectionType,
                    hardwareData.Timestamp,
                    hardwareData.AppVersion
                };

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving hardware data to database: {ex.Message}");
                throw;
            }
            finally
            {
                connection.CloseConnection();
            }
        }
    }
}
