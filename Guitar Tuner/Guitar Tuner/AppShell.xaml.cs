using Guitar_Tuner.ViewModels;
using Guitar_Tuner.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Guitar_Tuner
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(TunePage), typeof(TunePage));
        }

    }
}
