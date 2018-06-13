using Asteroids.ViewModels.Base;
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
    public class MainPage2ViewModel : UrhoViewModelBase<Game.UrhoApp>
    {


        public MainPage2ViewModel(INavigationService navigationService) 
            : 
            base (navigationService)
        {
        }


    }
}
