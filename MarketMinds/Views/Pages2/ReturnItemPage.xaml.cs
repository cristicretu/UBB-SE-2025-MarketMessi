using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel.DataAnnotations.Schema;
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
        public ReturnItemPage()
        {
            this.InitializeComponent();
        }
        private void Click_MoneyCheckBox(object sender, RoutedEventArgs e)
        {
            if (Moneyback_CheckBox.IsChecked == true)
            {
                Anotherproduct_CheckBox.IsChecked = false;
            }
        }

        private void Click_ProductCheckBox(object sender, RoutedEventArgs e)
        {
            if (Anotherproduct_CheckBox.IsChecked == true)
            {
                Moneyback_CheckBox.IsChecked = false;
            }
        }

        private void Click_Back(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AccountPage));
        }
        private void Click_Return_Item(object sender, RoutedEventArgs e)
        {
            if ((Moneyback_CheckBox.IsChecked == true || Anotherproduct_CheckBox.IsChecked == true) && Description_TextBox.Text != string.Empty)
            {
                Display_TextBlock.Text = "Request sent succsessfully!";

                //Add database connection and sql execution here
            }
            else
            {
                if (Moneyback_CheckBox.IsChecked == false && Anotherproduct_CheckBox.IsChecked == false)
                {
                    if (Description_TextBox.Text != string.Empty)
                    {
                        Display_TextBlock.Text = "Please check the approach you want!";
                    }
                    else
                    {
                        Display_TextBlock.Text = "Please fill enverything in before submiting!";
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
