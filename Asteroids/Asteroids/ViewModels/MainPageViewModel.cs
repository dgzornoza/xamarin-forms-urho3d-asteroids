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
    public class MainPageViewModel : UrhoViewModelBase<Game.UrhoApp>
    {


        public MainPageViewModel(INavigationService navigationService) 
            : 
            base (navigationService)
        {
        }



        private ICommand _go;
        public ICommand Go        {
            get => _go = (_go ?? new DelegateCommand(async () =>
            {
                await this.NavigationService.NavigateAsync("MainPage2");
            }));
        }


    }
}
