using System;
using System.Collections.Generic;
using System.Text;
using Urho.Forms;
using Xamarin.Forms;
using Asteroids.Helpers;

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

        ///// <summary>
        ///// Definicion de la propiedad enlazada con el timeline
        ///// </summary>
        //public static readonly BindableProperty TimelineProperty = BindableProperty.Create(
        //    nameof(Timeline),
        //    typeof(TimelineModel),
        //    typeof(UrhoSurfaceView),
        //    propertyChanged: (bindable, oldValue, newValue) =>
        //    {
        //        // eliminar el contenido si se elimina el timeline
        //        if (null == newValue) (bindable as SlidesViewer).Reset();
        //    }
        //);




        #endregion [Binding]




        #region [Propiedades]

        ///// <summary>
        ///// Propiedad para obtener o establecer el timeline
        ///// </summary>
        //public TimelineModel Timeline
        //{
        //    get { return (TimelineModel)GetValue(TimelineProperty); }
        //    set { SetValue(TimelineProperty, value); }
        //}



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
                // destroy UrhoSurface
                OnDestroy();
            }
        }

    }
}
