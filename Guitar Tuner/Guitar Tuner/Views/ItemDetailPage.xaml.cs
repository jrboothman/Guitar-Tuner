using Guitar_Tuner.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Guitar_Tuner.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}