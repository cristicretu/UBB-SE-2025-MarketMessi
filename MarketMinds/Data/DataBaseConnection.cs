using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataAccessLayer
{
    public class DataBaseConnection
    {
        private SqlConnection sqlConnection;
        private readonly string connectionString;
        private readonly IConfiguration configuration;

        public DataBaseConnection(IConfiguration currentConfiguration)
        {
            configuration = currentConfiguration;
            string? localDataSource = configuration["LocalDataSource"];
            string? initialCatalog = configuration["InitialCatalog"];

            connectionString = "Data Source=" + localDataSource + ";" +
                        "Initial Catalog=" + initialCatalog + ";" +
                       "Integrated Security=True;" +
                       "TrustServerCertificate=True";
            try
            {
                sqlConnection = new SqlConnection(connectionString);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error initializing SQL connection: {exception.Message}");
            }
        }

        public SqlConnection GetConnection()
        {
            return this.sqlConnection;
        }

        public void OpenConnection()
        {
            if (this.sqlConnection.State != System.Data.ConnectionState.Open)
            {
                this.sqlConnection.Open();
            }
        }

        public void CloseConnection()
        {
            if (this.sqlConnection.State == System.Data.ConnectionState.Open)
            {
                this.sqlConnection.Close();
            }
        }
    }
}