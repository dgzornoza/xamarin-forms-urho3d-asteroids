using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;
using Urho.Forms;
using Xamarin.Forms;
using Urho3D = Urho;

namespace Asteroids.ViewModels.Base
{
    public class UrhoViewModelBase<TUrhoApp> : ViewModelBase where TUrhoApp : Urho3D.Application
    {
        private UrhoSurface _urhoSurfaceInstance;


        public UrhoViewModelBase(INavigationService navigationService) 
            :
            base (navigationService)
        {
        }



        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            this._createUrhoSurface();
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            UrhoSurfaceInstance = null;
            UrhoSurface.OnDestroy();
        }


        

        public UrhoSurface UrhoSurfaceInstance
        {
            get => _urhoSurfaceInstance;
            private set => SetProperty(ref _urhoSurfaceInstance, value);
        }




        private void _createUrhoSurface()
        {
            if (null != UrhoSurfaceInstance) return;

            this._urhoSurfaceInstance = new UrhoSurface()
            {
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            
            // ensure initialization app
            this._urhoSurfaceInstance.SizeChanged += _sizeChanged;
            RaisePropertyChanged(nameof(UrhoSurfaceInstance));
        }


        private void _sizeChanged(object sender, EventArgs e)
        {
            this._urhoSurfaceInstance.SizeChanged -= _sizeChanged;

            // urho app options
            string assetsFolder;
            switch (Device.RuntimePlatform)
            {
                default:
                    assetsFolder = "Data";
                    break;
            }
            Urho3D.ApplicationOptions options = new Urho3D.ApplicationOptions(assetsFolder);

            // ensure initialization app
            Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await this._urhoSurfaceInstance.Show<TUrhoApp>(options);
                });

                // do not repeat
                return false;
            });
        }
    }
}
