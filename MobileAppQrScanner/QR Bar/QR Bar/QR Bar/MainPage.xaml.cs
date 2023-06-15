using MySqlConnector;
using QR_Bar.Models;
using System;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace QR_Bar
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            init();
        }

        void init()
        {
            /*BackgroundColor = Constants.backgroundColor;
            //label_username.TextColor = Constants.mainTextColor;
            //label_password.TextColor = Constants.mainTextColor;
            
            logoIcon.HeightRequest = Constants.loginIconHeigh;*/
            activitySpinner.IsVisible = false;

            entry_username.Completed += (s, e) => entry_password.Focus();
            entry_password.Completed += (s, e) => button_signin.Focus();
        }

        void SingInProcedure(object sender, EventArgs e)
        {            
            login(entry_username.Text.ToString(), entry_password.Text.ToString());
            entry_username.Text = "";
            entry_password.Text = "";
        }

        private async void login(string username, string pass)
        {
            try
            {
                //await DisplayAlert("Login Success", "Welcome Back " + username, "Next!");
                string login_querry = "SELECT ID, Name, Role_ID FROM users WHERE UserName = '" + username + "' and Pass = '" + pass + "';";

                MySqlConnection conn = new MySqlConnection(MySql.connection_string);
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(login_querry, conn);
                cmd.Connection = conn;

                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    dr.Read();

                    Login_Info.ID = int.Parse(dr[0].ToString());
                    Login_Info.Name = (dr[1].ToString());
                    Login_Info.Role = int.Parse(dr[2].ToString());
                    

                    await DisplayAlert("Login Success", "Welcome Back " + Login_Info.Name, "Next!");

                    if(Login_Info.Role == 1)
                    {
                        await Navigation.PushAsync(new Refill());
                    }
                    else
                    {
                        await Navigation.PushAsync(new Bartender());
                    }
                    
                }
                else
                {
                    await DisplayAlert("Login Failed", "Please Check Your Credentials", "Try Again!");
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Unexpected Error", ex.ToString(), "Try Again!");
                //conn.Close();
            }
        }
    }
}
