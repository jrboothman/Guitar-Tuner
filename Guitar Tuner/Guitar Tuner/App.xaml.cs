using Guitar_Tuner.Services;
using Guitar_Tuner.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Guitar_Tuner
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();


            DependencyService.Register<MockDataStore>();
            MainPage =new LoginPage();
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
