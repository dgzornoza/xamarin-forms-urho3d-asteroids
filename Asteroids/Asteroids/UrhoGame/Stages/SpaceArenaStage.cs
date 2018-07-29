using Asteroids.UrhoGame.Components;
using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Urho2D;
using XamarinForms.Toolkit.Urho3D;

namespace Asteroids.UrhoGame.Stages
{
    public class SpaceArenaStage : Component
    {
        public SpaceArenaStage() { }

        public override void OnSceneSet(Scene scene)
        {            
            base.OnSceneSet(scene);

            // attach to scene
            if (null != scene)
            {
                this._create();
            }
            // dettach from scene
            else
            {

            }
        }






        private void _create()
        {
            // create world
            PhysicsWorld2D physicsWorld2D = this.Scene.GetOrCreateComponent<PhysicsWorld2D>();
            physicsWorld2D.Gravity = Vector2.Zero;

            // create ship
            this.Node.CreateChild($"{nameof(Ship)}").CreateComponent<Ship>();
            // create asteroid
            this.Node.CreateChild($"{nameof(Asteroid)}").CreateComponent<Asteroid>();
        }
    }
}
