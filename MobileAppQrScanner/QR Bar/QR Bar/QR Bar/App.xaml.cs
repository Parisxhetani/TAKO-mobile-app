using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QR_Bar
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
            //Refilll = new NavigationPage(new Refill());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
