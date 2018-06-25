using Asteroids.ViewModels.Base;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Urho.Forms;
using Xamarin.Forms;

namespace Asteroids.ViewModels
{
    public class MainPageViewModel : UrhoViewModelBase<UrhoGame.UrhoApp>
    {


        public MainPageViewModel(INavigationService navigationService) 
            : 
            base (navigationService)
        {
        }



    }
}
