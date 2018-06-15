using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Urho2D;

namespace Asteroids.Game.Components
{
    public class Ship : Component
    {
        public Ship() { }

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

            var cache = Application.ResourceCache;
            Sprite2D sprite = cache.GetSprite2D("Textures/Ship.dds");
            if (sprite == null) return;
            Node spriteNode = Scene.CreateChild("StaticSprite2D");
            StaticSprite2D staticSprite = spriteNode.CreateComponent<StaticSprite2D>();
            staticSprite.Color = Color.Green;
            staticSprite.BlendMode = BlendMode.Alpha;
            staticSprite.Sprite = sprite;

            spriteNode.RunActionsAsync(new Urho.Actions.TintTo(2, Color.Blue));
        }
    }
}
