using System;
using System.Collections.Generic;
using System.Text;
using Urho.Forms;
using Xamarin.Forms;

namespace Asteroids.Controls
{
    public class UrhoSurfaceView : UrhoSurface
    {
        private Page _parentPage;
        private bool _isInitialized;

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
                if (null == newValue) (bindable as UrhoSurfaceView)._destroy();
            }
        );


        #endregion [Binding]




        #region [Propiedades]

        /// <summary>
        /// Property with Urho App
        /// </summary>
        public Urho.Application UrhoApp
        {
            get { return (Urho.Application)GetValue(UrhoAppProperty); }
            set { SetValue(UrhoAppProperty, value); }
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
                this._destroy();
            }
        }


    }
}
