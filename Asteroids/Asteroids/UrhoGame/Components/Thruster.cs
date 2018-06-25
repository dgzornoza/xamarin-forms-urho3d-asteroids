using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Asteroids.UrhoGame.Components
{
    public class Thruster : Component
    {
        public Thruster() { }

        public override void OnSceneSet(Scene scene)
        {
            base.OnSceneSet(scene);

            // attach to scene
            if (null != scene)
            {
                this._initialize();
            }
            // dettach from scene
            else
            {

            }
        }

        private void _initialize()
        {

        }
    }
}
