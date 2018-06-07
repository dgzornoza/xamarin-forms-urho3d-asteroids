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

        private UrhoSurface _urho;


        public MainPageViewModel(INavigationService navigationService) 
            : base (navigationService)
        {
            _initializeUrho();
        }





        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {

        }
        
        public override void Destroy()
        {
            UrhoSurface.OnDestroy();
        }


        public UrhoSurface Urho
        {
            get => _urho;
            private set => SetProperty(ref _urho, value);
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

            Urho = new UrhoSurface
            {
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Urho.Show<Game.UrhoApp>(new Urho.ApplicationOptions(assetsFolder));
                });
                
                return false;
            });
            
        }

    }
}
