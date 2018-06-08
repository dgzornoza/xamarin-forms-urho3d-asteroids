using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho.Forms;
using Xamarin.Forms;

namespace Asteroids.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {

        private UrhoSurface _urhoView;


        public MainPageViewModel(INavigationService navigationService) 
            : base (navigationService)
        {
            _initializeUrho();
        }





        private void _initializeUrho()
        {
            string assetsFolder;
            switch (Device.RuntimePlatform)
            {
                default:
                    assetsFolder = "Data";
                    break;
            }


            //Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            //{
            //    Device.BeginInvokeOnMainThread(async () =>
            //    {
            //        await Urho.Show<Game.UrhoApp>(new Urho.ApplicationOptions(assetsFolder));
            //    });

            //    return false;
            //});


            //Task.Run(() => { UrhoView.Show<Game.UrhoApp>(new Urho.ApplicationOptions("Data")); }).Wait();

        }

    }
}
