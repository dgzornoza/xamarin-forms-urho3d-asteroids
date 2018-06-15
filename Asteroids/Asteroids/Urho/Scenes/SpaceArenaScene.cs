using Asteroids.Game.Components;
using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Asteroids.Game.Scenes
{
    public class SpaceArenaScene : Component
    {
        private Node _rootNode;

        public SpaceArenaScene() { }

        public override void OnSceneSet(Scene scene)
        {            
            base.OnSceneSet(scene);

            // attach to scene
            if (null != scene)
            {
                this._rootNode = Node.CreateChild(nameof(SpaceArenaScene));
                this._create();
            }
            // dettach from scene
            else
            {
                Node.RemoveChild(this._rootNode);
                this._rootNode = null;
            }
        }


        private void _create()
        {
            // TEMP: load rube
            //string filePath = ResourceCache.GetResourceFileName("Urho2D/RubePhysics/documentA.json");
            //Toolkit.Urho.Rube.B2dJson b2dJson = new Toolkit.Urho.Rube.B2dJson();
            //b2dJson.ReadIntoNodeFromFile(filePath, this._scene.CreateChild("physicsNode"), out string errorMsg);

            // create ship
            this._rootNode.CreateChild("ship").CreateComponent<Ship>();
        }
    }
}
