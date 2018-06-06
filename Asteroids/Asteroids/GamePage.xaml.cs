using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho.Forms;
using Xamarin.Forms;

namespace Asteroids
{
    public partial class GamePage : ContentPage
    {

        public GamePage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            UrhoSurface.OnDestroy();
            base.OnDisappearing();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            string assetsFolder;
            switch (Device.RuntimePlatform)
            {
                default:
                    assetsFolder = "Data";
                    break;
            }

            await MainUrhoSurface.Show<Game.UrhoApp>(new Urho.ApplicationOptions(assetsFolder));
        }

    }
}
