using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Marketplace_SE.Data
{
    public class Database
    {
        public static Database database { get; set; }
        private SqlConnection connection;
        private string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool Connect()
        {
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Close()
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        public void Execute(string query, string[] parameters, object[] values)
        {
            SqlCommand command = new SqlCommand(query, connection);
            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue(parameters[i], values[i]);
            }
            command.ExecuteNonQuery();
        }

        public DataTable Get(string query, string[] parameters, object[] values)
        {
            SqlCommand command = new SqlCommand(query, connection);
            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue(parameters[i], values[i]);
            }
            
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            
            return dataTable;
        }

        public List<T> ConvertToObject<T>(DataTable dataTable) where T : new()
        {
            List<T> objects = new List<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                T obj = new T();
                foreach (var prop in typeof(T).GetFields())
                {
                    if (dataTable.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.FieldType));
                    }
                }
                objects.Add(obj);
            }
            return objects;
        }
    }
}

namespace Marketplace_SE.Objects
{
    public class User
    {
        public int id { get; private set; }
        public string username { get; private set; }
        public string password { get; private set; }

        public User(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public void SetId(int id)
        {
            this.id = id;
        }
    }
}

namespace Marketplace_SE.Utilities
{
    public class Notification
    {
        public Microsoft.UI.Xaml.Controls.Button OkButton { get; private set; }
        private Microsoft.UI.Window window;

        public Notification(string title, string message)
        {
            // Simple implementation
            window = new Microsoft.UI.Window();
            OkButton = new Microsoft.UI.Xaml.Controls.Button();
        }

        public Microsoft.UI.Window GetWindow()
        {
            return window;
        }
    }

    public class DataEncoder
    {
        public static DateTime ConvertTimestampToLocalDateTime(long timestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(timestamp).ToLocalTime();
            return dateTime;
        }
    }
}

// Windows.UI and Microsoft.UI namespaces
namespace Windows.UI.Text
{
    public enum FontStyle
    {
        Normal,
        Italic
    }
}

namespace Microsoft.UI
{
    public class Window
    {
        public void Activate() { }
        public void Close() { }
    }
    
    public class Colors
    {
        public static object Black { get; } = new object();
        public static object Green { get; } = new object();
        public static object Red { get; } = new object();
    }
}

namespace Microsoft.UI.Text
{
    public class FontWeights
    {
        public static object Bold { get; } = new object();
    }
}

// Basic user order class
namespace Marketplace_SE
{
    public class UserOrder
    {
        public int id;
        public string name;
        public string description;
        public decimal cost;
        public long created;
        public string orderStatus;
        public int sellerId;
        public int buyerId;
    }
}

namespace Microsoft.UI.Xaml.Controls
{
    public class Button
    {
        public object Content { get; set; }
        public event EventHandler Click;
    }

    public class Page
    {
        public Microsoft.UI.Xaml.Controls.Frame Frame { get; protected set; }
    }

    public class Frame
    {
        public void Navigate(Type pageType)
        {
            // Navigation implementation
        }
    }
} 