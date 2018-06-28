
using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Gui;
using Urho.Urho2D;

namespace Asteroids.UrhoGame.Components
{
    public class NodeTextInfo : Component
    {
        private Text _textElement;

        RigidBody2D _rigidBody;
        ParticleEmitter _particleEmitter;

        public NodeTextInfo()
        {
            this.ReceiveSceneUpdates = true;
        }

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

        public HorizontalAlignment HorizontalTextAlignment
        {
            get => this._textElement.HorizontalAlignment;
            set => this._textElement.HorizontalAlignment = value;
        }

        public VerticalAlignment VerticalTextAlignment
        {
            get => this._textElement.VerticalAlignment;
            set => this._textElement.VerticalAlignment = value;
        }





        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            this._showDebugInfo();
        }




        private void _initialize()
        {
            this._rigidBody = this.Node.GetComponent<RigidBody2D>(true);
            this._particleEmitter = this.Node.GetComponent<ParticleEmitter>(true);

            // text for show node info
            this._textElement = new Text();
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
            // RigidBody2D
            if (null != this._rigidBody)
            {
                this._textElement.Value = $"RigidBody2D:\r\n" +
                    $"AngularDamping: {this._rigidBody.AngularDamping}\r\n" +
                    $"AngularVelocity: {this._rigidBody.AngularVelocity}\r\n" +
                    $"Inertia: {this._rigidBody.Inertia}\r\n" +
                    $"LinearVelocity: {this._rigidBody.LinearVelocity}\r\n" +
                    $"LinearDamping: {this._rigidBody.LinearDamping}\r\n" +
                    $"Mass: {this._rigidBody.Mass}\r\n" +
                    $"\r\n\r\n";
            }

            // ParticleEmitter
            if (null != this._particleEmitter)
            {
                this._textElement.Value = $"ParticleEmitter:\r\n" +
                    $"NumParticles: {this._particleEmitter.NumParticles}\r\n" +
                        $"\tParticleEffect:\r\n" +
                        $"\tDampingForce: {this._particleEmitter.Effect.DampingForce}\r\n" +
                        $"\tParticleSize: {this._particleEmitter.Effect.MinParticleSize} {this._particleEmitter.Effect.MaxParticleSize}\r\n" +
                        $"\tVelocity: {this._particleEmitter.Effect.MinVelocity} {this._particleEmitter.Effect.MaxVelocity}\r\n" +
                        $"\tEmissionRate: {this._particleEmitter.Effect.MinEmissionRate} {this._particleEmitter.Effect.MaxEmissionRate}\r\n" +
                        $"\tTimeToLive: {this._particleEmitter.Effect.MinTimeToLive} {this._particleEmitter.Effect.MaxTimeToLive}\r\n" +
                        $"\tSize: Add:{this._particleEmitter.Effect.SizeAdd} Mul:{this._particleEmitter.Effect.SizeMul}\r\n" +
                        $"\r\n\r\n";
            }
        }
    }
}
