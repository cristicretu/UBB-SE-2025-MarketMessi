using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using DataAccessLayer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReturnItemPage : Page
    {
        private DataBaseConnection dbConnection;

        public ReturnItemPage()
        {
            this.InitializeComponent();

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfiguration config = builder.Build();
            dbConnection = new DataBaseConnection(config);
        }

        private void Click_MoneyCheckBox(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Moneyback_CheckBox.IsChecked == true)
            {
                Anotherproduct_CheckBox.IsChecked = false;
            }
        }

        private void Click_ProductCheckBox(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Anotherproduct_CheckBox.IsChecked == true)
            {
                Moneyback_CheckBox.IsChecked = false;
            }
        }

        private void Click_Back(object sender, RoutedEventArgs routedEventArgs)
        {
            Frame.Navigate(typeof(AccountPage));
        }
        private void Click_Return_Item(object sender, RoutedEventArgs routedEventArgs)
        {
            // added the database connection and the query to insert the return request into the database
            if ((Moneyback_CheckBox.IsChecked == true || Anotherproduct_CheckBox.IsChecked == true) && !string.IsNullOrWhiteSpace(Description_TextBox.Text))
            {
                try
                {
                    dbConnection.OpenConnection();
                    using (var conn = dbConnection.GetConnection())
                    {
                        string query = "INSERT INTO ReturnRequests (Description, Type, DateRequested) VALUES (@description, @type, @date)";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@description", Description_TextBox.Text);
                            cmd.Parameters.AddWithValue("@type", Moneyback_CheckBox.IsChecked == true ? "Moneyback" : "Replacement");
                            cmd.Parameters.AddWithValue("@date", DateTime.Now);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    Display_TextBlock.Text = "Request sent successfully!";
                }
                catch (Exception itemReturnException)
                {
                    Display_TextBlock.Text = "Error: " + itemReturnException.Message;
                }
                finally
                {
                    dbConnection.CloseConnection();
                }
            }
            else
            {
                if (Moneyback_CheckBox.IsChecked == false && Anotherproduct_CheckBox.IsChecked == false)
                {
                    if (!string.IsNullOrWhiteSpace(Description_TextBox.Text))
                    {
                        Display_TextBlock.Text = "Please check the approach you want!";
                    }
                    else
                    {
                        Display_TextBlock.Text = "Please fill everything in before submitting!";
                    }
                }
                else
                {
                    Display_TextBlock.Text = "Please describe the reason you wish to return the item!";
                }
            }
        }
    }
}
