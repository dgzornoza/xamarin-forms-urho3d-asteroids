using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Asteroids.Game.Components
{
    public class AsteroidScene : Component
    {
        public AsteroidScene() { }

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
