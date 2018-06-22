
using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Gui;
using Urho.Urho2D;

namespace Asteroids.Game.Components
{
    public class NodeTextInfo : Component
    {
        private Text _textElement;

        RigidBody2D _rigidBody;

        public NodeTextInfo() { }

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
                this._destroy();
            }
        }



        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            this._showDebugInfo();
        }




        private void _initialize()
        {
            _rigidBody = this.Node.GetComponent<RigidBody2D>(true);

            // text for show node info
            this._textElement = new Text()
            {                
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            this._textElement.SetColor(Color.White);
            _textElement.SetFont(this.Application.ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), 15);

            // add to ui layout
            this.Application.UI.Root.AddChild(_textElement);
        }

        private void _destroy()
        {
            // remove from ui
            this.Application.UI.Root.RemoveChild(_textElement);
        }


        private void _showDebugInfo()
        {
            
            if (null == _rigidBody) return;

            this._textElement.Value = $"AngularDamping: {_rigidBody.AngularDamping}\r\n" +
                $"AngularVelocity: {_rigidBody.AngularVelocity}\r\n" +
                $"Inertia: {_rigidBody.Inertia}\r\n" +
                $"LinearVelocity: {_rigidBody.LinearVelocity}\r\n" +
                $"LinearDamping: {_rigidBody.LinearDamping}\r\n" +
                $"Mass: {_rigidBody.Mass}\r\n";
        }
    }
}
