using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Guitar_Tuner.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TunePage : ContentPage
    {
        public TunePage()
        {
            InitializeComponent();
        }

        protected async void StandardClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StandardTuning());
        }

        protected async void DropDClicked(object sender, EventArgs e) 
        {
            await Navigation.PushAsync(new DropDTuning());
        }

        protected async void DropCClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DropCTune());
        }
    }
}