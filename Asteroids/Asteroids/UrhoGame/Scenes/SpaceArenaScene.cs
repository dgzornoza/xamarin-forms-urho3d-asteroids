using Asteroids.UrhoGame.Components;
using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Urho2D;

namespace Asteroids.UrhoGame.Scenes
{
    public class SpaceArenaScene : Component
    {
        public SpaceArenaScene() { }

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
            // TEMP: load rube
            //string filePath = ResourceCache.GetResourceFileName("Urho2D/RubePhysics/documentA.json");
            //Toolkit.Urho.Rube.B2dJson b2dJson = new Toolkit.Urho.Rube.B2dJson();
            //b2dJson.ReadIntoNodeFromFile(filePath, this._scene.CreateChild("physicsNode"), out string errorMsg);

            // create world
            PhysicsWorld2D physicsWorld2D = this.Scene.GetOrCreateComponent<PhysicsWorld2D>();
            physicsWorld2D.Gravity = Vector2.Zero;
            
            // create ship
            this.Node.CreateChild($"{nameof(Ship)}-node").CreateComponent<Ship>();
            // create asteroid
            this.Node.CreateChild($"{nameof(Asteroid)}-node").CreateComponent<Asteroid>();
        }
    }
}
