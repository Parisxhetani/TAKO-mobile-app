using MySqlConnector;
using QR_Bar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace QR_Bar
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Refill : ContentPage
    {
        public Refill()
        {
            InitializeComponent();
            init();

            credits_entry.Text = "";
            activity_Waitting_Spinner.IsVisible = false;
        }

        void init()
        {
            credits_entry.Completed += (s, e) => scan_button.Focus();
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

                    insert_refill_record(qr_code_id, qr_code_current_credits);
                }
                else
                {
                    credits_entry.Text = "";
                    activity_Waitting_Spinner.IsVisible = false;
                    await DisplayAlert("Operation Failed", "QR Code Does Not Exists", "Try Again!");
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                credits_entry.Text = "";
                activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Unexpected Error", ex.ToString(), "Try Again!");
                //conn.Close();
            }
        }

        private async void insert_refill_record(int qr_code_id, int qr_code_current_credits)
        {
            try
            {
                int add_amount = int.Parse(credits_entry.Text);
                int new_balance = qr_code_current_credits + add_amount;

                string querry = "INSERT INTO refills (Amount, User_ID, QR_ID) VALUES (" + add_amount + ", " + Login_Info.ID + ", " + qr_code_id + ");" +
                    "UPDATE qr_details SET Credits = " + new_balance + " WHERE ID = " + qr_code_id + ";";

                MySqlConnection conn = new MySqlConnection(MySql.connection_string);
                MySqlCommand cmd = new MySqlCommand(querry, conn);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                credits_entry.Text = "";
                activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Operation Succeeded", "" + add_amount + "ALL were added to account. Total balance " + new_balance + "ALL. ", "Ok!");
            }
            catch (Exception ex)
            {
                credits_entry.Text = "";
                activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Unexpected Error", ex.ToString(), "Try Again!");
                //conn.Close();
            }
        }

        private async void Info_Clicked(object sender, EventArgs e)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            var result = await scanner.Scan();
            find_qr_value(result.ToString());
        }

        private async void find_qr_value(string scan_value)
        {
            try
            {
                string querry = "SELECT Credits FROM qr_details WHERE Scan_Value = '" + scan_value + "';";

                MySqlConnection conn = new MySqlConnection(MySql.connection_string);
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(querry, conn);
                cmd.Connection = conn;

                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    dr.Read();

                    //int qr_code_id = int.Parse(dr[0].ToString());
                    int qr_code_current_credits = int.Parse(dr[0].ToString());
                    await DisplayAlert("Info", qr_code_current_credits + " ALL", "Ok!");
                    conn.Close();

                }
                else
                {
                    credits_entry.Text = "";
                    activity_Waitting_Spinner.IsVisible = false;
                    await DisplayAlert("Operation Failed", "QR Code Does Not Exists", "Try Again!");
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                credits_entry.Text = "";
                activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Unexpected Error", ex.ToString(), "Try Again!");
                //conn.Close();
            }
        }

        private async void Cash_Out_Clicked(object sender, EventArgs e)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            var result = await scanner.Scan();

            find_withdraw_amount(result.ToString());               
        }

        private async void find_withdraw_amount(string scan_value)
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

                    save_withdraw(qr_code_id, qr_code_current_credits);

                }
                else
                {
                    credits_entry.Text = "";
                    activity_Waitting_Spinner.IsVisible = false;
                    await DisplayAlert("Operation Failed", "QR Code Does Not Exists", "Try Again!");
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                credits_entry.Text = "";
                activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Unexpected Error", ex.ToString(), "Try Again!");
                //conn.Close();
            }
        }

        private async void save_withdraw(int qr_code_id, int qr_code_current_credits)
        {
            try
            {
                string querry = "INSERT INTO withdraws (Credits, User_ID, QR_ID) VALUES (" + qr_code_current_credits + ", " + Login_Info.ID + ", " + qr_code_id + ");" +
                    "UPDATE qr_details SET Credits = 0 WHERE ID = " + qr_code_id + ";";

                MySqlConnection conn = new MySqlConnection(MySql.connection_string);
                MySqlCommand cmd = new MySqlCommand(querry, conn);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                credits_entry.Text = "";
                activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Operation Succeeded", "Return " + qr_code_current_credits + " ALL", "Ok!");
            }
            catch (Exception ex)
            {
                credits_entry.Text = "";
                activity_Waitting_Spinner.IsVisible = false;
                await DisplayAlert("Unexpected Error", ex.ToString(), "Try Again!");
                //conn.Close();
            }
        }
    }
}