using Asteroids.UrhoGame.Components;
using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Urho2D;
using XamarinForms.Toolkit.Urho3D;

namespace Asteroids.UrhoGame.Stages
{
    public class SpaceArenaStage : BaseComponent
    {
        public SpaceArenaStage() { }



        protected override void _initialize()
        {
            base._initialize();

            // create world
            PhysicsWorld2D physicsWorld2D = this.Scene.GetOrCreateComponent<PhysicsWorld2D>();
            physicsWorld2D.Gravity = Vector2.Zero;

            // create ship
            this.Node.CreateChild($"{nameof(Player)}").CreateComponent<Player>();
            // create asteroid
            this.Node.CreateChild($"{nameof(Asteroid)}").CreateComponent<Asteroid>();

        }

        protected override void _destroy()
        {
            base._destroy();
        }




    }
}
