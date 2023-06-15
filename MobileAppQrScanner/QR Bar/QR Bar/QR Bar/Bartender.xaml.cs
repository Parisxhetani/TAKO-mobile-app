using MySqlConnector;
using QR_Bar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace QR_Bar
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Bartender : ContentPage
    {
        private List<Product> products_list_from_DB = new List<Product> { };
        private List<Product> products_list_to_display = new List<Product> { };

        public Bartender()
        {
            InitializeComponent();




            get_all_products_from_DB();
        }

        private async void get_all_products_from_DB()
        {
            try
            {
                string querry = "SELECT * FROM products";

                MySqlConnection conn = new MySqlConnection(MySql.connection_string);
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(querry, conn);
                cmd.Connection = conn;

                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    products_list_from_DB.Clear();

                    while (dr.Read())
                    {
                        Product p = new Product();

                        p.ID = int.Parse(dr[0].ToString());
                        p.Name = dr[1].ToString();
                        p.Price = int.Parse(dr[2].ToString());
                        p.Quantity = 0;

                        products_list_from_DB.Add(p);
                    }

                    conn.Close();

                    products_listView.ItemsSource = null;
                    products_listView.ItemsSource = products_list_from_DB;
                    products_list_to_display = products_list_from_DB;
                }
                else
                {
                    await DisplayAlert("Product list is empty!", "There are no products available at the time.", "Ok!");
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Unexpected Error", ex.ToString(), "Ok!");
                //conn.Close();
            }
        }

        private void products_listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var product = e.Item as Product;

            foreach (var p in products_list_to_display)
            {
                if (p.ID == product.ID)
                {
                    p.Quantity = p.Quantity + 1;

                    int curent_total = int.Parse(total_label.Text);
                    int new_total = curent_total + p.Price;
                    total_label.Text = new_total.ToString();

                    refresh_list();
                }
            }
        }

        private void refresh_list()
        {
            products_listView.ItemsSource = null;
            products_listView.ItemsSource = products_list_to_display;
        }

        private void products_listView_Refreshing(object sender, EventArgs e)
        {
            foreach (var p in products_list_to_display)
            {
                p.Quantity = 0;
                refresh_list();
            }
            total_label.Text = 0.ToString();
            products_listView.EndRefresh();

        }

        private async void scan_button_Clicked(object sender, EventArgs e)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            var result = await scanner.Scan();

            find_qr_id(result.ToString());               
        }

        private async void find_qr_id(string scan_value)
        {
            try
            {
                string querry = "SELECT ID, Credits FROM qr_details WHERE Scan_Value = '" + scan_value + "';";

                MySqlConnection conn = new MySqlConnection(MySql.connection_string);
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(querry, conn);
                cmd.Connection = conn;

                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    dr.Read();

                    int qr_code_id = int.Parse(dr[0].ToString());
                    int qr_code_current_credits = int.Parse(dr[1].ToString());

                    conn.Close();

                    if (qr_code_current_credits >= int.Parse(total_label.Text))
                    {
                        foreach (var p in products_list_to_display)
                        {
                            if (p.Quantity > 0)
                            {
                                int success = insert_sale(qr_code_id, p.ID, p.Quantity);
                                if (success == -1)
                                {
                                    await DisplayAlert("Unexpected Error", "", "Try Again!");
                                    break;
                                }
                            }
                        }
                        update_credits(qr_code_id, qr_code_current_credits);
                    }
                    else
                    {
                        await DisplayAlert("Not Enough Credits", "This user doesn't have enough credits", "Try Again!");
                    }
                }
                else
                {
                    total_label.Text = 0.ToString();
                    //activity_Waitting_Spinner.IsVisible = false;
                    await DisplayAlert("Operation Failed", "QR Code Does Not Exists", "Try Again!");
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                total_label.Text = 0.ToString();
                //activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Unexpected Error", ex.ToString(), "Try Again!");
                //conn.Close();
            }
        }

        private async void update_credits(int qr_code_id, int qr_code_current_credits)
        {
            try
            {
                int sub_amount = int.Parse(total_label.Text);
                int new_balance = qr_code_current_credits - sub_amount;

                string querry = "UPDATE qr_details SET Credits = " + new_balance + " WHERE ID = " + qr_code_id + ";";

                MySqlConnection conn = new MySqlConnection(MySql.connection_string);
                MySqlCommand cmd = new MySqlCommand(querry, conn);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                total_label.Text = 0.ToString();
                //activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Operation Succeeded", "Total balance " + new_balance + "ALL. ", "Ok!");
                get_all_products_from_DB();
            }
            catch (Exception ex)
            {
                total_label.Text = 0.ToString();
                //activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Unexpected Error", ex.ToString(), "Try Again!");
                //conn.Close();
            }
        }

        private int insert_sale(int qr_code_id, int product_ID, int product_quantity)
        {
            try
            {
                string querry = "INSERT INTO sales (User_ID, Product_ID, Quantity, QR_ID) VALUES(" + Login_Info.ID + ", " + product_ID + ", " + product_quantity + ", " + qr_code_id + ")";

                MySqlConnection conn = new MySqlConnection(MySql.connection_string);
                MySqlCommand cmd = new MySqlCommand(querry, conn);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                return 1;
            }
            catch (Exception ex)
            {
                //activity_Waitting_Spinner.IsVisible = false;
                DisplayAlert("Unexpected Error", ex.ToString(), "Try Again!");
                //conn.Close();
                return -1;
            }
        }
    }
}