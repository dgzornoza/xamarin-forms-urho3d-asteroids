using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Forms;
using Xamarin.Forms;

namespace Asteroids.Controls
{
    /// <summary>
    /// Enum with Urho Surface
    /// </summary>
    public enum EnumUrhoSurfaceState
    {
        NONE = 0,
        RUN,
        PAUSE,
        STOP
    }

    public class UrhoSurfaceView : UrhoSurface
    {


        /// <summary>
        /// default constructor
        /// </summary>
        public UrhoSurfaceView()
        {
            // initialize properties                 
            VerticalOptions = LayoutOptions.FillAndExpand;

            this.SizeChanged += _sizeChanged;
        }



        #region [Binding]

        /// <summary>
        /// Property for set Urho App
        /// </summary>
        public static readonly BindableProperty UrhoAppProperty = BindableProperty.Create(
            nameof(UrhoApp),
            typeof(Urho.Application),
            typeof(UrhoSurfaceView),
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                // destroy urho app if not exists new value
                if (null == newValue) (bindable as UrhoSurfaceView)._destroyUrhoApp();
            }
        );

        /// <summary>
        /// Property for set UrhoSurface state
        /// </summary>
        public static readonly BindableProperty StateProperty = BindableProperty.Create(
            nameof(State),
            typeof(EnumUrhoSurfaceState),
            typeof(UrhoSurfaceView),
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                UrhoSurfaceView instance = (bindable as UrhoSurfaceView);

                if (null == newValue || !instance.IsUrhoAppActive) return;

                switch ((EnumUrhoSurfaceState)newValue)
                {
                    case EnumUrhoSurfaceState.RUN: OnResume(); break;
                    case EnumUrhoSurfaceState.PAUSE: OnPause(); break;
                    case EnumUrhoSurfaceState.STOP: instance._destroyUrhoApp(); break;                    
                }
            }
        );

        #endregion [Binding]




        #region [Propiedades]

        public bool IsUrhoAppActive => Urho.Application.HasCurrent && Urho.Application.Current.IsActive;

        /// <summary>
        /// Property with Urho App
        /// </summary>
        public Urho.Application UrhoApp
        {
            get { return (Urho.Application)GetValue(UrhoAppProperty); }
            set { SetValue(UrhoAppProperty, value); }
        }

        /// <summary>
        /// Property with UrhoSurface state
        /// </summary>
        public EnumUrhoSurfaceState State
        {
            get { return (EnumUrhoSurfaceState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }


        #endregion [Propiedades]


        private void _sizeChanged(object sender, EventArgs e)
        {
            this.SizeChanged -= _sizeChanged;

            Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await this.Show<Game.UrhoApp>(new Urho.ApplicationOptions("Data"));
                });

                // do not repeat
                return false;
            });            
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (null == Parent)
            {
                this._destroyUrhoApp();
            }
        }


        private void _destroyUrhoApp()
        {
            OnDestroy();
        }


    }
}
